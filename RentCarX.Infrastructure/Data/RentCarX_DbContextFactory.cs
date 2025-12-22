using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RentCarX.Infrastructure.Helpers.Development;
using RentCarX.Infrastructure.Settings;

namespace RentCarX.Infrastructure.Data 
{
    public sealed class RentCarX_DbContextFactory : IDesignTimeDbContextFactory<RentCarX_DbContext>
    {
        public RentCarX_DbContextFactory() { }

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
            string connectionString = ConnectionString.GetConnectionString(configuration);
            optionsBuilder.UseSqlServer(connectionString);

            var adminRoleSettings = configuration.GetSection("IdentityAdminRole").Get<IdentityAdminRole>() 
                ?? new IdentityAdminRole { Password = "DefaultPasswordForMigration" };

            IOptions<IdentityAdminRole> adminRoleOptions = Options.Create(adminRoleSettings);

            return new RentCarX_DbContext(optionsBuilder.Options, adminRoleOptions);
        }
    }
}