using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.CQRS.Queries.Admin.GetPendingReservations;

public sealed class GetPendingReservationsQueryHandler : IRequestHandler<GetPendingReservationsQuery, List<Domain.Models.Reservation?>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<GetPendingReservationsQueryHandler> _logger;

    public GetPendingReservationsQueryHandler(IPaymentRepository paymentRepository, ILogger<GetPendingReservationsQueryHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<List<Domain.Models.Reservation?>>  Handle(GetPendingReservationsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get pending reservations...");

        var pendingReservations = await _paymentRepository
            .GetPendingReservations()
            .Select(p => p.Reservation)
            .ToListAsync(cancellationToken);

        return pendingReservations.Any() ? pendingReservations : [];
    }
}
