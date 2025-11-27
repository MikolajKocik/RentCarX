using Microsoft.Extensions.Options;
using RentCarX.Application.DTOs.Stripe;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Interfaces.Services.Stripe;
using RentCarX.Domain.Models.Stripe;
using RentCarX.Infrastructure.Settings;
using Stripe.Checkout;

namespace RentCarX.Infrastructure.Services.Stripe
{
    public sealed class PaymentService : IPaymentService
    {
        private readonly SessionService _sessionService;
        private readonly IReservationRepository _reservationRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICarRepository _carRepository;
        private readonly StripeSettings _settings;

        public PaymentService(
            SessionService sessionService,
            IReservationRepository reservationRepository,
            IPaymentRepository paymentRepository,
            ICarRepository carRepository,
            IOptions<StripeSettings> stripeOptions)
        {
            _sessionService = sessionService;
            _reservationRepository = reservationRepository;
            _paymentRepository = paymentRepository;
            _carRepository = carRepository;
            _settings = stripeOptions.Value;
        }

        public async Task<string> CreateCheckoutSessionAsync(
            CreateCheckoutSessionRequest request, 
            CancellationToken ct = default
            )
        {
            var reservation = await _reservationRepository.GetReservationByIdAsync(request.ReservationId, ct);
            if (reservation is null)
                throw new NotFoundException("Reservation not found.", request.ReservationId.ToString());

            var car = await _carRepository.GetCarByIdAsync(reservation.CarId, ct);
            if (car is null) 
                throw new NotFoundException("Car associated with the reservation not found.", reservation.CarId.ToString());
            if (string.IsNullOrEmpty(car.StripePriceId))
                throw new InvalidOperationException("Car is not synchronized with Stripe (missing price ID).");

            var days = (reservation.EndDate.Date - reservation.StartDate.Date).Days;
            if (days <= 0) days = 1;
            var amount = car.PricePerDay * days;

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = _settings.SuccessUrl,
                CancelUrl = _settings.CancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "reservationId", reservation.Id.ToString() }
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        Price = car.StripePriceId,
                        Quantity = days
                    }
                }
            };

            var session = await _sessionService.CreateAsync(options, cancellationToken: ct);

            var payment = new Payment
            {
                StripeCheckoutSessionId = session.Id,
                Amount = amount,
                Currency = "pln",
                Status = PaymentStatus.Pending,
                ReservationId = reservation.Id,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment, ct);

            return session.Url!;
        }

        public async Task HandleCheckoutSessionCompletedAsync(
            string sessionId,
            string? paymentIntentId = null,
            string? customerId = null,  
            CancellationToken ct = default)
        {
            var payment = await _paymentRepository.GetBySessionIdAsync(sessionId, ct);
            if (payment is null) return;
            if (payment.Status == PaymentStatus.Succeeded) return;

            payment.Status = PaymentStatus.Succeeded;
            payment.SucceededAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(paymentIntentId))
                payment.StripePaymentIntentId = paymentIntentId;
            if (!string.IsNullOrEmpty(customerId))
                payment.StripeCustomerId = customerId;

            if (payment.ReservationId.HasValue)
            {
                var reservation = await _reservationRepository.GetReservationByIdAsync(payment.ReservationId.Value, ct);
                if (reservation is not null)
                {
                    reservation.IsPaid = true;
                    await _reservationRepository.UpdateAsync(reservation, ct);
                }
            }

            await _paymentRepository.UpdateAsync(payment, ct);
        }

        public async Task HandleRefundSucceededAsync(
            string refundId,
            string? chargeId = null,
            long? amount = null,
            string? currency = null,
            CancellationToken ct = default
            )
        {
            var payment = await _paymentRepository.GetByRefundIdAsync(refundId, ct);
            if (payment is null || payment.Status == PaymentStatus.Refunded) return;

            payment.Status = PaymentStatus.Refunded;
            payment.RefundedAt = DateTime.UtcNow;

            if (payment.ReservationId.HasValue)
            {
                var reservation = await _reservationRepository.GetReservationByIdAsync(payment.ReservationId.Value, ct);
                if (reservation is not null)
                {
                    reservation.IsPaid = false;
                    await _reservationRepository.UpdateAsync(reservation, ct);
                }
            }

            await _paymentRepository.UpdateAsync(payment, ct);
        }
    }
}
