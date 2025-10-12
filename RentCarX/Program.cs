using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using RentCarX.Application.Extensions;
using RentCarX.Infrastructure.Data;
using RentCarX.Infrastructure.Extensions;
using RentCarX.Presentation.Extensions;
using RentCarX.Presentation.Middleware;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"Environment Name: {builder.Environment.EnvironmentName}");

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.AddPresentation();

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
    options.GroupNameFormat = "'v'VVV";
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
            .PersistKeysToFileSystem(new DirectoryInfo(@"C:\DataProtectionKeys"));
}

var app = builder.Build();

Console.WriteLine($"App Environment Name: {app.Environment.EnvironmentName}");

// serilog

app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

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

app.MapControllers();

// auto-migrating
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<RentCarX_DbContext>();

    var pendingMigrations = dbContext.Database.GetPendingMigrations();

    if (pendingMigrations.Any())
    {
        dbContext.Database.Migrate();
    }
}

await app.RunAsync();