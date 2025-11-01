using Hangfire;
using RentCarX.Domain.Models;
using RentCarX.HangfireWorker.Jobs;

namespace RentCarX.HangfireWorker.Common;

public sealed class HangfireJobScheduler : IJobScheduler
{
    public void SetJob(Reservation reservation)
    {
        BackgroundJob.Schedule<SendReservationDeadline>(
          job => job.SendReminderAsync(CancellationToken.None),
          reservation.EndDate.AddMinutes(-30));
    }
}
