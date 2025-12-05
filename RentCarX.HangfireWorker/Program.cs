using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RentCarX.HangfireWorker.Jobs;
using RentCarX.Infrastructure.Extensions;
using RentCarX.Infrastructure.Helpers.Development;
using RentCarX.Infrastructure.Helpers.Production;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        string connectionString = context.HostingEnvironment.IsDevelopment()
            ? ConnectionString.GetConnectionString(context.Configuration)
            : AzureSqlConfiguration.GetConnectionString(context.Configuration);

        services.AddInfrastructure(context.Configuration, context.HostingEnvironment, connectionString);

        services.AddHangfire(config =>
            config.UseSqlServerStorage(connectionString));

        services.AddHangfireServer();

        services.AddTransient<UpdateCarAvailabilityJob>();
    })
    .Build();


using (var scope = host.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    var recurringJobManager = serviceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<UpdateCarAvailabilityJob>(
        "update-car-availability",
        job => job.ExecuteAsync(CancellationToken.None),
        Cron.Minutely);
}

await host.RunAsync();
    