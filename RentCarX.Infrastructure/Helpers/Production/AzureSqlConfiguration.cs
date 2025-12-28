using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentCarX.Infrastructure.Data;
using RentCarX.Infrastructure.Settings;
using System.Diagnostics;

namespace RentCarX.Infrastructure.Helpers.Production;

public static class AzureSqlConfiguration
{
    public static void SetDbContext(IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = GetConnectionString(configuration);

        services.AddDbContext<RentCarX_DbContext>(options =>
            options.UseSqlServer(connectionString));
    }
    public static string GetConnectionString(IConfiguration configuration)
    {
        var sqlSettings = configuration.GetSection("AzureSqlConnection")
            .Get<AzureSqlConnection>()
                ?? throw new Exception("No azure settings are available for sql database.");

        var builder = new SqlConnectionStringBuilder()
        {
            DataSource = sqlSettings.DataSource,
            UserID = sqlSettings.UserID,
            Password = sqlSettings.Password,
            InitialCatalog = sqlSettings.InitialCatalog,
            Encrypt = true,
            TrustServerCertificate = false,
            ConnectTimeout = 60
        };

        return builder.ConnectionString;
    }
}
