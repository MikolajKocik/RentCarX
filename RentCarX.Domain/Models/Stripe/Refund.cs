using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Domain.Models.Stripe
{
    public sealed class Refund
    {
        public int Id { get; set; }
        public string StripeRefundId { get; set; } = string.Empty; 
        public decimal Amount { get; set; }
        public string Status { get; set; } = "succeeded"; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int PaymentId { get; set; }
        public Payment? Payment { get; set; }
    }
}
