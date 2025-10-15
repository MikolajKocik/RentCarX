using Microsoft.Extensions.Configuration;

namespace RentCarX.Infrastructure.Helpers
{
    public static class ConnectionString
    {
        public static string GetConnectionString(IConfiguration configuration)
        {

            var database = configuration.GetSection("Database");
            string server = database["Server"]!;
            string databaseName = database["DatabaseName"]!;
            string username = database["Username"]!;
            string password = database["Password"]!;
            string port = database["Port"]!;

            ConnectionStringValidation.CheckParameters(server, databaseName, username, password, port);

            return $"Server={server},{port};Database={databaseName};User Id={username};Password={password};Encrypt=True;TrustServerCertificate=True;";
        }
    }
}
