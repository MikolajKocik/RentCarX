using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RentCarX.Application.Interfaces.JWT;
using RentCarX.Application.Services.User;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Interfaces.Services.Stripe;
using RentCarX.Domain.Interfaces.UserContext;
using RentCarX.Infrastructure.Data;
using RentCarX.Infrastructure.Helpers.Development;
using RentCarX.Infrastructure.Helpers.Production;
using RentCarX.Infrastructure.Repositories;
using RentCarX.Infrastructure.Services.JWT;
using RentCarX.Infrastructure.Services.Stripe;
using RentCarX.Infrastructure.Settings;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;

namespace RentCarX.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
        )
    {

        if (environment.IsDevelopment())
        {
            string connectionString = ConnectionString.GetConnectionString(configuration);

            Debug.WriteLineIf(connectionString != null, "Connection string was found successfully");

            services.AddDbContext<RentCarX_DbContext>(options =>
              options
                 .UseSqlServer(connectionString)
                 .EnableSensitiveDataLogging());
        }

        if (environment.IsProduction())
        {
            AzureSqlConfiguration.SetDbContext(services, configuration);
        }

        // add identity
        services.AddIdentity<User, IdentityRole<Guid>>()
           .AddEntityFrameworkStores<RentCarX_DbContext>()
           .AddDefaultTokenProviders();


        services.AddScoped<ICarRepository, CarRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContextService, UserContextService>();

        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        services.AddScoped<IStripeProductService, StripeProductService>();
        services.AddScoped<IPaymentService, PaymentService>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // stripe section
        services.Configure<StripeSettings>(configuration.GetSection("Stripe"));
        var stripeSettings = configuration.GetSection("Stripe")
          .Get<StripeSettings>()
              ?? throw new Exception("No stripe settings are available for payment");

        // register Stripe client from stripe.net
        services.AddSingleton(new StripeClient(stripeSettings.SecretKey));

        // register services from stripe.net that require a client
        services.AddScoped(sp => new ProductService(sp.GetRequiredService<StripeClient>()));
        services.AddScoped(sp => new PriceService(sp.GetRequiredService<StripeClient>()));
        services.AddScoped(sp => new SessionService(sp.GetRequiredService<StripeClient>()));

        // webhook handler
        services.AddScoped<IStripeWebhookHandler, StripeWebhookHandlerImplementation>();
    }
}
