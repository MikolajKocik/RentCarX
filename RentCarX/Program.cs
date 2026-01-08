using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using RentCarX.Application.Extensions;
using RentCarX.HangfireWorker.Extensions;
using RentCarX.Infrastructure.Data;
using RentCarX.Infrastructure.Extensions;
using RentCarX.Infrastructure.Helpers.Development;
using RentCarX.Infrastructure.Helpers.Production;
using RentCarX.Infrastructure.Settings;
using RentCarX.Presentation.Extensions;
using RentCarX.Presentation.Helpers;
using RentCarX.Presentation.Middleware;
using RentCarX.Presentation.Observability.Prometheus;
using Serilog;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

Debug.WriteLine($"Environment Name: {builder.Environment.EnvironmentName}");

string connectionString = builder.Environment.IsDevelopment()
    ? ConnectionString.GetConnectionString(builder.Configuration)
    : AzureSqlConfiguration.GetConnectionString(builder.Configuration);
Debug.WriteLine(connectionString);

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment, connectionString);
builder.Services.AddHangfireWorker(builder.Configuration, connectionString);
builder.Services.AddApplication(builder.Configuration);

builder.AddPresentation();

builder.Services.AddMemoryCache();

builder.Services.AddApiVersioning(options =>
{
    // 'api-supported-versions'
    options.ReportApiVersions = true;

    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;

    // version is read from a fragment of url
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    // formatting version in swagger's UI
    options.GroupNameFormat = ApiVersioningHelper.GroupNameFormat;
    // automatic version substitution in routing templates
    options.SubstituteApiVersionInUrl = true;
});


if (builder.Environment.IsDevelopment())
{
    // Ensures that encryption keys survives while application restarts (important for logging)
    builder.Services.AddDataProtection();
    // user secrets data
    builder.Configuration.AddUserSecrets<Program>();
}
else
{
    builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")));

    builder.Services.Configure<AzureSqlConnection>(
        builder.Configuration.GetSection("AzureSqlConnection"));
}

var app = builder.Build();

#region Middlewares

// exceptions middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// serilog
app.UseSerilogRequestLogging();

// middleawre for buffering body request
app.Use((context, next) =>
{
    context.Request.EnableBuffering();
    return next();
});

// midleware for tempo traces
app.Use(async (context, next) =>
{
    using var activity = ApplicationMetrics.ActivitySource.StartActivity(
        $"HTTP {context.Request.Method} {context.Request.Path}");

    await next();

    activity?.SetTag("http.response.status_code", context.Response.StatusCode);
});

// jti middleware
app.UseMiddleware<TokenBlackListMiddleware>();

// middleware for prometheus metrics
app.Use(async (context, next) =>
{
    var sw = Stopwatch.StartNew();
    await next.Invoke();
    sw.Stop();

    ApplicationMetrics.RequestDuration.Record(sw.Elapsed.TotalMilliseconds);
});

Console.WriteLine($"App Environment Name: {app.Environment.EnvironmentName}");

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

#endregion

// swagger 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
        c.RoutePrefix = "swagger";
    });

    // Configure the HTTP request pipeline.
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
            Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
    });
}

app.MapControllers();


// auto-migrating
using (IServiceScope scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<RentCarX_DbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
        {
            IEnumerable<string> pendingMigrations = dbContext.Database.GetPendingMigrations();

            if (pendingMigrations.Any())
            {
                dbContext.Database.Migrate();
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occured while migrate the pending migrations");
        throw;
    }

    try
    {
        IOptions<IdentityAdminRole> role = scope.ServiceProvider.GetRequiredService<IOptions<IdentityAdminRole>>();
        IdentityAdminRole? admin = role?.Value;

        string email = admin?.Email ?? string.Empty;
        string password = admin?.Password ?? string.Empty;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            throw new ArgumentNullException("Roles are not configurated for admin account");

        await IdentitySeeder.SeedUserAsync(scope.ServiceProvider);
        await IdentitySeeder.SeedAdminAsync(scope.ServiceProvider, email, password);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occured while seeding the identity roles");
        throw;
    }
}

await app.RunAsync();