using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Domain.Models.Stripe
{
    public sealed class Payment
    {
        public int Id { get; set; }
        public string StripePaymentIntentId { get; set; } = string.Empty;
        public string? StripeCheckoutSessionId { get; set; }
        public string? StripeCustomerId { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SucceededAt { get; set; }
        public DateTime? RefundedAt { get; set; }

        public Guid UserId { get; set; }
        public User? User { get; set; }

        public Guid? ReservationId { get; set; }
        public Reservation? Reservation { get; set; }

        public int? ItemId { get; set; }
        public Item? Item { get; set; }
    }
}
