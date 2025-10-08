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
Console.WriteLine($"Is Development: {builder.Environment.IsDevelopment()}");

// modu³y rozszerzajace program.cs

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.AddPresentation();

// -------------------------------

builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(@"C:\DataProtectionKeys"));

if (builder.Environment.IsDevelopment())
{
    // user secrets data
    builder.Configuration.AddUserSecrets<Program>();
}

var app = builder.Build();

Console.WriteLine($"App Environment Name: {app.Environment.EnvironmentName}");
Console.WriteLine($"App Is Development: {app.Environment.IsDevelopment()}");

// serilog

app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

if (app.Environment.IsDevelopment())
{
    // debugowanie uwierzytelniania jwt

    app.Use(async (context, next) =>
    {
        Console.WriteLine("\n--- Check HttpContext.User after authentication ---");

        // HttpContext.User jest ustawiany przez middleware uwierzytelniania
        var user = context.User;

        if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
        {
            Console.WriteLine($"User Authenticated: YES");
            Console.WriteLine($"Authentication type: {user.Identity.AuthenticationType ?? "Empty"}");

            Console.WriteLine("User claims:");
            bool isAdminClaimPresent = false;
            if (user.Claims != null)
            {
                foreach (var claim in user.Claims)
                {
                    Console.WriteLine($"- Type: {claim.Type}, Value: {claim.Value}");
                    if (claim.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && claim.Value == "Admin")
                    {
                        isAdminClaimPresent = true;
                    }
                }
            }
            else
            {
                Console.WriteLine("No claims assigned to the user.");
            }

            Console.WriteLine($"Claim's role 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' with value 'Admin' found: {isAdminClaimPresent}");
            Console.WriteLine($"Result IsInRole('Admin'): {user.IsInRole("Admin")}"); // opiera siê na claimach typu ClaimTypes.Role

        }
        else
        {
            Console.WriteLine("User not authenticated.");
            if (user != null && user.Identity != null)
            {
                Console.WriteLine($"Identity.IsAuthenticated: {user.Identity.IsAuthenticated}");
                Console.WriteLine($"Identity.AuthenticationType: {user.Identity.AuthenticationType ?? "Empty"}");
            }
            else if (user == null)
            {
                Console.WriteLine("HttpContext.User jest null.");
            }
            else if (user.Identity == null)
            {
                Console.WriteLine("HttpContext.User.Identity jest null.");
            }
        }

        Console.WriteLine("-------------------------------------------------------");

        await next();
    });
}

app.UseAuthorization();

// swagger 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RentCarX API V1");
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