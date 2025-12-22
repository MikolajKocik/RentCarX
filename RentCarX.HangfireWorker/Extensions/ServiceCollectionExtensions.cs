using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentCarX.Application.Interfaces.Services.Hangfire;
using RentCarX.HangfireWorker.Common;

namespace RentCarX.HangfireWorker.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddHangfireWorker(
            this IServiceCollection services,
            IConfiguration configuration, 
            string connectionString
            )
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString));

            services.AddHangfireServer();

            services.AddSingleton<IJobScheduler, HangfireJobScheduler>();
        }
    }
}