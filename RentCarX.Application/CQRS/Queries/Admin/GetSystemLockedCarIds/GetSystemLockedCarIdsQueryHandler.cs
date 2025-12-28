using MediatR;
using Microsoft.Extensions.Logging;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.CQRS.Queries.Admin.GetSystemLockedCarIds;

public sealed class GetSystemLockedCarIdsQueryHandler : IRequestHandler<GetSystemLockedCarIdsQuery, List<Guid>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<GetSystemLockedCarIdsQueryHandler> _logger;

    public GetSystemLockedCarIdsQueryHandler(IPaymentRepository paymentRepository, ILogger<GetSystemLockedCarIdsQueryHandler> logger)
    {
        _paymentRepository = paymentRepository; 
        _logger = logger;
    }

    public async Task<List<Guid>> Handle(GetSystemLockedCarIdsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get system locked ids...");
        var lockedIds = await _paymentRepository.GetLockedCarIdsAsync(cancellationToken);
    
        return lockedIds.Any() ? lockedIds : [];
    }
}
