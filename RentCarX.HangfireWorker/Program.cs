using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RentCarX.HangfireWorker.Jobs;
using RentCarX.Infrastructure.Extensions;
using RentCarX.Infrastructure.Helpers.Development;
using RentCarX.Infrastructure.Helpers.Production;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        string hangfireConnectionString = string.Empty;

        if (context.HostingEnvironment.IsDevelopment())
        {
            hangfireConnectionString = ConnectionString.GetConnectionString(context.Configuration);
        }
        else
        {
            IConfiguration configuration = context.Configuration;
            hangfireConnectionString = AzureSqlConfiguration.GetConnectionString(configuration);
        }

        services.AddHangfire(config =>
            config.UseSqlServerStorage(hangfireConnectionString));

        services.AddHangfireServer();

        services.AddInfrastructure(context.Configuration, context.HostingEnvironment);

        services.AddTransient<UpdateCarAvailabilityJob>();
    })
    .Build();
    
RecurringJob.AddOrUpdate<UpdateCarAvailabilityJob>(
    "update-car-availability",
    job => job.ExecuteAsync(CancellationToken.None),
    Cron.Minutely);

await host.RunAsync();
    