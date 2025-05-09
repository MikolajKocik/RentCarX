using Microsoft.EntityFrameworkCore;
using RentCarX.Application.Extensions;
using RentCarX.Domain.Models;
using RentCarX.Infrastructure.Data;
using RentCarX.Infrastructure.Extensions;
using RentCarX.Presentation.Extensions;
using RentCarX.Presentation.Middleware;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.AddPresentation();

if (builder.Environment.IsDevelopment())
{
    // user secrets data
    builder.Configuration.AddUserSecrets<Program>();
}

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RentCarX API V1");
        c.RoutePrefix = "swagger"; 
    });

    app.UseDeveloperExceptionPage(); // middleware
}

app.UseSerilogRequestLogging();

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

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGroup("api/identity")
    .WithTags("Identity")
    .MapIdentityApi<User>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

