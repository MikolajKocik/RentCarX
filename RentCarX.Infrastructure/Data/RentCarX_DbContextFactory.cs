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

            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                                   ?? configuration.GetConnectionString("DefaultConnection"); 

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DB_CONNECTION_STRING' environment variable or 'DefaultConnection' in appsettings.json is not set.");
            }

            optionsBuilder.UseSqlServer(connectionString);

            return new RentCarX_DbContext(optionsBuilder.Options);
        }
    }
}