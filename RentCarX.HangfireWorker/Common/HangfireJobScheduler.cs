using Hangfire;
using RentCarX.Domain.Models;
using RentCarX.HangfireWorker.Jobs;
using RentCarX.Application.Interfaces.Services.Hangfire;

namespace RentCarX.HangfireWorker.Common;

public sealed class HangfireJobScheduler : IJobScheduler
{
    public void SetJob(Reservation reservation)
    { 
        BackgroundJob.Schedule<SendReservationDeadline>(
          job => job.ExecuteAsync(CancellationToken.None),
          reservation.EndDate.AddMinutes(-30));
    }
}
