using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentCarX.Application.DTOs.Stripe;
using RentCarX.Application.Helpers;
using RentCarX.Application.Interfaces.Services.NotificationStrategy;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Interfaces.Services.Stripe;
using RentCarX.Domain.Models;
using RentCarX.Domain.Models.Stripe;
using RentCarX.Infrastructure.Settings;
using Stripe;
using Stripe.Checkout;

namespace RentCarX.Infrastructure.Services.Stripe;

public sealed class PaymentService : IPaymentService
{
    private readonly SessionService _sessionService;
    private readonly IReservationRepository _reservationRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICarRepository _carRepository;
    private readonly StripeSettings _settings;
    private readonly IEnumerable<INotificationSender> _senders;
    private readonly ILogger<PaymentService> _logger;
    private readonly InvoiceService _invoiceService;
    private readonly RefundService _refundService;

    public PaymentService(
        SessionService sessionService,
        IReservationRepository reservationRepository,
        IPaymentRepository paymentRepository,
        ICarRepository carRepository,
        IOptions<StripeSettings> stripeOptions,
        IEnumerable<INotificationSender> senders,
        ILogger<PaymentService> logger,
        InvoiceService invoiceService,
        RefundService refundService
        )
    {
        _sessionService = sessionService;
        _reservationRepository = reservationRepository;
        _paymentRepository = paymentRepository;
        _carRepository = carRepository;
        _settings = stripeOptions.Value;
        _senders = senders;
        _logger = logger;
        _refundService = refundService;
        _invoiceService = invoiceService;
    }

    public async Task<string> CreateCheckoutSessionAsync(
        CreateCheckoutSessionRequest request, 
        CancellationToken ct = default
        )
    {
        Reservation? reservation = await _reservationRepository.GetReservationByIdAsync(request.ReservationId, ct);
        if (reservation is null)
            throw new NotFoundException("Reservation not found.", request.ReservationId.ToString());

        Car? car = await _carRepository.GetCarByIdAsync(reservation.CarId, ct);
        if (car is null) 
            throw new NotFoundException("Car associated with the reservation not found.", reservation.CarId.ToString());
        if (string.IsNullOrEmpty(car.StripePriceId))
            throw new InvalidOperationException("Car is not synchronized with Stripe (missing price ID).");

        int days = (reservation.EndDate.Date - reservation.StartDate.Date).Days;
        if (days <= 0) days = 1;
        decimal amount = car.PricePerDay * days;

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = _settings.SuccessUrl,
            CancelUrl = _settings.CancelUrl,
            InvoiceCreation = new SessionInvoiceCreationOptions
            {
                Enabled = true,
            },
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

        Session session = await _sessionService.CreateAsync(options, cancellationToken: ct);

        var payment = new Payment
        {
            StripeCheckoutSessionId = session.Id,
            Amount = amount,
            Currency = "pln",
            Status = PaymentStatus.Pending,
            ReservationId = reservation.Id,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow,
            CheckoutUrl = session.Url,
            Item = new Item
            {
                Name = $"{car.Brand} {car.Model}",
                Description = $"Rental car - {car.FuelType}, {car.Year}",
                Price = amount,
                Currency = "pln"
            }
        };

        await _paymentRepository.AddAsync(payment, ct);

        return session.Url!;
    }

    public async Task CreateRefundAsync(string paymentIntentId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(paymentIntentId))
            throw new BadRequestException("Brak identyfikatora płatności (PaymentIntentId) do dokonania zwrotu.");

        var options = new RefundCreateOptions
        {
            // pi.. key from database
            PaymentIntent = paymentIntentId,
        };

        try
        {
            await _refundService.CreateAsync(options, cancellationToken: ct);
            _logger.LogInformation("Successfully initiated Stripe refund for PaymentIntent: {Id}", paymentIntentId);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error during refund for PaymentIntent: {Id}", paymentIntentId);
            throw new BadRequestException($"Stripe Refund Error: {ex.Message}");
        }
    }

    public async Task HandleCheckoutSessionCompletedAsync(
        string sessionId,
        string? paymentIntentId = null,
        string? customerId = null,  
        string? invoiceId = null,
        CancellationToken ct = default)
    {
        Payment? payment = await _paymentRepository.GetBySessionIdAsync(sessionId, ct);
        if (payment is null || payment.Status == PaymentStatus.Succeeded) return;

        payment.Status = PaymentStatus.Succeeded;
        payment.SucceededAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(paymentIntentId))
            payment.StripePaymentIntentId = paymentIntentId;
        if (!string.IsNullOrEmpty(customerId))
            payment.StripeCustomerId = customerId;

        if (payment.ReservationId.HasValue)
        {
            Reservation? reservation = await _reservationRepository.GetReservationByIdAsync(payment.ReservationId.Value, ct);
            if (reservation is not null)
            {
                reservation.IsPaid = true;
                await _reservationRepository.UpdateAsync(reservation, ct);
            }
        }

        await _paymentRepository.UpdateAsync(payment, ct);

        string? invoiceUrl = null;
        if (!string.IsNullOrEmpty(invoiceId))
        {
            try
            {
                Invoice invoice = await _invoiceService.GetAsync(invoiceId, cancellationToken: ct);
                invoiceUrl = invoice.HostedInvoiceUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not retrieve invoie");
            }
        }

        string subject = "RentCarX - Payment Successful!";
        string invoiceLinkHtml = !string.IsNullOrEmpty(invoiceUrl)
            ? $"<p><a href='{invoiceUrl}'>Click here to download your Invoice</a></p>"
            : "<p>Your invoice will be available in your profile shortly.</p>";

        string itemName = payment.Item?.Name ?? "Car Reservation";

        string messageBody = $"""
        <html>
          <body>
              <h2>Payment Confirmed!</h2>
              <p>Great news! Your payment for <strong>{itemName}</strong> was successful.</p>
              <p>Amount paid: <strong>{payment.Amount} {payment.Currency.ToUpper()}</strong>.</p>
              {invoiceLinkHtml}
              <p>Thank you for choosing RentCarX!</p>
          </body>
        </html>
        """;
        
        INotificationSender smtp = _senders.First(s => s.StrategyName.Equals(NotificationStrategyOptions.Smtp));
        await smtp.SendNotificationAsync(subject, messageBody, ct, payment.User?.Email);
    }

    public async Task HandleRefundSucceededAsync(
        string refundId,
        string? chargeId = null,
        long? amount = null,
        string? currency = null,
        CancellationToken ct = default
        )
    {
        Payment? payment = await _paymentRepository.GetByRefundIdAsync(refundId, ct);
        if (payment is null || payment.Status == PaymentStatus.Refunded) return;

        payment.Status = PaymentStatus.Refunded;
        payment.RefundedAt = DateTime.UtcNow;

        if (payment.ReservationId.HasValue)
        {
            Reservation? reservation = await _reservationRepository.GetReservationByIdAsync(payment.ReservationId.Value, ct);
            if (reservation is not null)
            {
                reservation.IsPaid = false;
                await _reservationRepository.UpdateAsync(reservation, ct);
            }
        }

        await _paymentRepository.UpdateAsync(payment, ct);
    }

    public async Task HandlePaymentFailedAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        Payment? payment = await _paymentRepository.GetByPaymentIntentIdAsync(paymentIntentId, cancellationToken);
        if (payment is null) return;

        payment.Status = PaymentStatus.Failed;
        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        string subject = "RentCarX - Payment Failed";
        string itemName = payment.Item?.Name ?? "Car Reservation";
        string messageBody = $"""
        <html>
          <body>
                <p>
                    Unfortunately, your payment for your car reservation <strong>{itemName}</strong> was unsuccessful.
                    The funds were not debited from your account. This could be due to insufficient funds, a card error, or an interrupted transaction.

                    Please try again or use a different payment method.
                    Total amount to pay: <strong>{payment.Amount} {payment.Currency.ToUpper()}</strong>.
                    <a href="{payment.CheckoutUrl}">Click here to try again</a>

                    If you experience any problems, please contact our customer service.

                    Thank you for using our services.
                </p>
          </body>
        </html>
        """;

        INotificationSender smtp = _senders.First(s => s.StrategyName.Equals(NotificationStrategyOptions.Smtp));
        await smtp.SendNotificationAsync(subject, messageBody, cancellationToken, payment.User?.Email);
    }

    public async Task HandleCheckoutSessionExpiredAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        Payment? payment = await _paymentRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        if (payment is null || payment.Status == PaymentStatus.Succeeded) return;

        payment.Status = PaymentStatus.Canceled;

        if (payment.ReservationId.HasValue)
        {
            Reservation? reservation = await _reservationRepository.GetReservationByIdAsync(payment.ReservationId.Value, cancellationToken);

            if (reservation is not null)
            {
                reservation.Car.IsAvailableFlag = 1;
                reservation.IsPaid = false;

                await _reservationRepository.UpdateAsync(reservation, cancellationToken);

                _logger.LogInformation("Reservation {Id} canceled due to session timeout.", reservation.Id);
            }
        }

        await _paymentRepository.UpdateAsync(payment, cancellationToken);
    }
}