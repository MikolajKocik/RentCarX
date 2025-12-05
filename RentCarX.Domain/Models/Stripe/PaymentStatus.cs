using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Domain.Models.Stripe
{
    public enum PaymentStatus
    {
        Pending = 0,
        RequiresAction = 1,
        Succeeded = 2,
        Failed = 3,
        Refunded = 4
    }
}
