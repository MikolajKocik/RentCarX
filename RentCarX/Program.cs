using Microsoft.EntityFrameworkCore;
using RentCarX.Application.Extensions;
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

app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

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

app.Run();

