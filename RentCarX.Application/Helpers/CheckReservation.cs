using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Models;
using System.Collections.Concurrent;

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

    // parallel atomic operation
    public static bool TryReserveCar(Car car)
    {
        return car.TryReserve();
    }
}
