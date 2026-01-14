using Microsoft.Extensions.Logging;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Models.Enums;
using RentCarX.HangfireWorker.Jobs.Abstractions;

namespace RentCarX.HangfireWorker.Jobs;

public sealed class UpdateReservationStatusJob : JobPlanner
{
    private readonly IReservationRepository _reservationRepository;

    public UpdateReservationStatusJob(
        IReservationRepository reservationRepository,
        ILogger<UpdateCarAvailabilityJob> logger
        ) : base(logger)
    {
        _reservationRepository = reservationRepository;
    }

    public override async Task PerformJobAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // reservations started --> status in progress
        await _reservationRepository.UpdateReservationStatusAsync(
                r => r.Status == ReservationStatus.Confirmed &&
                     r.StartDate <= now &&
                     r.EndDate > now &&
                     !r.IsDeleted,
                ReservationStatus.InProgress,
                cancellationToken);

        // reservations ended --> status completed
        await _reservationRepository.UpdateReservationStatusAsync(
                r => r.Status == ReservationStatus.InProgress &&
                     r.EndDate <= now &&
                     !r.IsDeleted,
                ReservationStatus.Completed,
                cancellationToken);
    }
}
