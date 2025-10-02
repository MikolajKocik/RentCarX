using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Domain.Interfaces.Services.Stripe
{
    public interface IStripeProductService
    {
        Task SyncProductsFromCarsAsync(CancellationToken cancellationToken = default);
    }

}
