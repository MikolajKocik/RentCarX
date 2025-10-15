using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RentCarX.HangfireWorker.Jobs;
using RentCarX.Infrastructure.Extensions;
using RentCarX.Infrastructure.Helpers;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        string connectionString = ConnectionString.GetConnectionString(context.Configuration);

        services.AddHangfire(config =>
          config.UseSqlServerStorage(connectionString));

        services.AddHangfireServer();

        services.AddInfrastructure(context.Configuration, context.HostingEnvironment);

        services.AddTransient<UpdateCarAvailabilityJob>();
    })
    .Build();
    
RecurringJob.AddOrUpdate<UpdateCarAvailabilityJob>(
    "update-car-availability",
    job => job.RunAsync(CancellationToken.None),
    Cron.Minutely);

await host.RunAsync();
    