using RentCarX.Domain.Models;

namespace RentCarX.Application.Interfaces.Services.Hangfire;

public interface IJobScheduler
{
    void SetJob(Reservation reservation);
}
