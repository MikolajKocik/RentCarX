using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RentCarX.Infrastructure.Data 
{
    public class RentCarX_DbContextFactory : IDesignTimeDbContextFactory<RentCarX_DbContext>
    {
        public RentCarX_DbContext CreateDbContext(string[] args)
        {
            // Configuration design-time due to environment variables not being available

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory())) 
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) 
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true, reloadOnChange: true) 
                .AddEnvironmentVariables() 
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<RentCarX_DbContext>();

            var database = configuration.GetSection("Database");
            string server = database["Server"]!;
            string databaseName = database["DatabaseName"]!;
            string username = database["Username"]!;
            string password = database["Password"]!;
            string port = database["Port"]!;

            string connectionString = $"Server={server},{port};Database={databaseName};User Id={username};Password={password};Encrypt=True;TrustServerCertificate=True;";


            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string is not set.");
            }

            optionsBuilder.UseSqlServer(connectionString);

            return new RentCarX_DbContext(optionsBuilder.Options);
        }
    }
}