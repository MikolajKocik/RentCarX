using Hangfire;
using RentCarX.Application.Interfaces.Services.Hangfire;
using RentCarX.Domain.Models;
using RentCarX.HangfireWorker.Jobs;
using System.Linq.Expressions;

namespace RentCarX.HangfireWorker.Common;

public sealed class HangfireJobScheduler : IJobScheduler
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireJobScheduler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void SetJob(Reservation reservation)
    {
        Expression<Action<SendReservationDeadline>> methodCall = job => 
            job.ExecuteAsync(CancellationToken.None);

        var delay = reservation.EndDate.AddMinutes(-30);
        _backgroundJobClient.Schedule(methodCall, delay);
    }
}
