using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Domain.Models.Stripe
{
    public sealed class StripeCustomer
    {
        public int Id { get; set; }
        public string StripeCustomerId { get; set; } = string.Empty;
        public Guid? UserId { get; set; } 
        public User? User { get; set; }

    }
}
