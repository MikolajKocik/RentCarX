using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.HangfireWorker.Jobs
{
    public sealed class UpdateCarAvailabilityJob
    {
        private readonly ICarRepository _carRepository;
        private readonly IReservationRepository _reservationRepository;

        public UpdateCarAvailabilityJob(
            ICarRepository carRepository,
            IReservationRepository reservationRepository
            )
        {
            _carRepository = carRepository;
            _reservationRepository = reservationRepository;
        }

        /// <summary>
        /// Updates the availability status of cars based on their current reservation status.
        /// </summary>
        /// <remarks>This method identifies cars that are marked as unavailable but do not have any active
        /// reservations  and updates their availability status to make them available. The operation is performed
        /// asynchronously  and respects the provided <paramref name="cancellationToken"/> to allow for
        /// cancellation.</remarks>
        /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            // active reservations
            var carIdsWithActiveReservations = await _reservationRepository
                .GetCarIdsWithActiveReservationAsync(now, cancellationToken);

            // search cars where cars are unavailable but have no active reservations
            var unavailableCars = await _carRepository.GetUnavailableCarsAsync(cancellationToken);

            // comparing two lists and get cars to make available
            var carsIdsToMakeAvailable = unavailableCars
                .Select(c => c.Id)
                .Except(carIdsWithActiveReservations)
                .ToList();

            if (carsIdsToMakeAvailable.Any())
            {
                await _carRepository.UpdateAvailabilityForCarsAsync(carsIdsToMakeAvailable, true, cancellationToken);
            }
        }
    }
}
