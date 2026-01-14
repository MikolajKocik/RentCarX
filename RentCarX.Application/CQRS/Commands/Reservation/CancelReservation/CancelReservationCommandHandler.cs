using MediatR;
using Microsoft.Extensions.Logging;
using RentCarX.Domain.ExceptionModels;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.DbContext;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Interfaces.Services.Stripe;
using RentCarX.Domain.Interfaces.UserContext;
using RentCarX.Domain.Models.Enums;
using RentCarX.Domain.Models.Stripe;

namespace RentCarX.Application.CQRS.Commands.Reservation.CancelReservation;

public sealed class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRentCarX_DbContext _context;
    private readonly ILogger<CancelReservationCommandHandler> _logger;
    private readonly IUserContextService _userService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentService _paymentService;

    public CancelReservationCommandHandler(
        IReservationRepository reservationRepository,
        IRentCarX_DbContext context,
        ILogger<CancelReservationCommandHandler> logger,
        IUserContextService userService,
        IPaymentRepository paymentRepository,
        IPaymentService paymentService
        )
    {
        _reservationRepository = reservationRepository;
        _context = context;
        _logger = logger;
        _paymentRepository = paymentRepository;
        _userService = userService;
        _paymentService = paymentService;
    }

    public async Task Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            throw new BadRequestException("Reservation id is invalid");

        Domain.Models.Reservation? reservation = await _reservationRepository.GetReservationByIdAsync(request.Id, cancellationToken);
        if (reservation is null)
            throw new NotFoundException("Reservation not found", nameof(request.Id)); 

        if (reservation.UserId != _userService.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to cancel reservation {ResId} owned by another user.", _userService.UserId, request.Id);
            throw new ForbiddenException("You don't have permission to cancel this reservation", nameof(reservation.Id));
        }

        if (reservation.StartDate <= DateTime.UtcNow)
            throw new BadRequestException("Cannot cancel a reservation that has already started or is in the past.");

        using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        try
        {
            Payment? payment = await _paymentRepository.GetByReservationId(reservation.Id, cancellationToken);

            if (payment != null && !string.IsNullOrEmpty(payment.StripePaymentIntentId))
            {
                await _paymentService.CreateRefundAsync(payment.StripePaymentIntentId, cancellationToken);

                payment.Status = PaymentStatus.Refunded;
                payment.RefundedAt = DateTime.UtcNow;

                _logger.LogInformation("Stripe refund initiated for PaymentIntent: {Pi}", payment.StripePaymentIntentId);
            }

            reservation.Status = ReservationStatus.Cancelled;
            reservation.IsDeleted = true;

            if (reservation.Car is not null)
            {
                reservation.Car.IsAvailableFlag = 1;
            }

            reservation.IsPaid = false;

            await _reservationRepository.SaveToDatabase(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Reservation {Id} was successfully cancelled.", request.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error occurred while cancelling the reservation {Id}", request.Id);
            throw;
        }
    }
}
