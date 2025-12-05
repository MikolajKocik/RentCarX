using Microsoft.Extensions.Logging;
using RentCarX.HangfireWorker.Interfaces;
using System.Diagnostics;

namespace RentCarX.HangfireWorker.Jobs.Abstractions;

public abstract class JobPlanner : IBackgroundJobTask
{
    protected readonly ILogger _logger;

    protected JobPlanner(ILogger logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var jobName = this.GetType().Name;

        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("--- [START] Starting job: {JobName} ---", jobName);

        try
        {
            await PerformJobAsync(cancellationToken);

            _logger.LogInformation("--- [SUCCESS] Job {JobName} completed successfully ---", jobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "--- [ERROR] Error occurred during job process {JobName} ---", jobName);
            throw; 
        }
        finally
        {
            stopwatch.Stop();

            _logger.LogInformation(
                "[END OF PROCESS] {JobName} completed with time: {Elapsed}ms",
                jobName,
                stopwatch.ElapsedMilliseconds);
        }
    }

    public abstract Task PerformJobAsync(CancellationToken cancellationToken);
}
