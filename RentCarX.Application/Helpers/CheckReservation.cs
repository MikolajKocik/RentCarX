using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.Helpers;

public static class CheckReservation
{
    public static bool IsReservationMarkedAsDeleted(
        Guid id,
        IReservationRepository repository
        )
    {
        return repository.GetDeletedReservations(id).Any();
    }
}
