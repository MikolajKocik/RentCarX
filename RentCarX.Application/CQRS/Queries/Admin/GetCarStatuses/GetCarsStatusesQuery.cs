using MediatR;
using RentCarX.Application.DTOs.Car;

namespace RentCarX.Application.CQRS.Queries.Admin.GetCarStatuses;

public record class GetCarsStatusesQuery() : IRequest<ICollection<CarStatusDto>>;