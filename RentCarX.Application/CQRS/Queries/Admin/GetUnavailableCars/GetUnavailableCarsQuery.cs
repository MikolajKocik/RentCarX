using MediatR;
using RentCarX.Application.DTOs.Car;

namespace RentCarX.Application.CQRS.Queries.Admin.GetUnavailableCars;

public sealed record GetUnavailableCarsQuery() : IRequest<List<CarStatusDto>>;
