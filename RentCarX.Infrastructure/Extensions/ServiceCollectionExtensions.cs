using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RentCarX.Application.Interfaces.JWT;
using RentCarX.Application.Services.User;
using RentCarX.Domain.Interfaces.DbContext;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Interfaces.Services.Stripe;
using RentCarX.Domain.Interfaces.UserContext;
using RentCarX.Infrastructure.Data;
using RentCarX.Infrastructure.Repositories;
using RentCarX.Infrastructure.Services.JWT;
using RentCarX.Infrastructure.Services.Stripe;
using RentCarX.Infrastructure.Settings;
using Stripe;
using Stripe.Checkout;

namespace RentCarX.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string connectionString
        )
    {
        services.AddDbContext<RentCarX_DbContext>(options =>
          options
             .UseSqlServer(connectionString, options =>
             {
                 options.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null
                );
             })
             .EnableSensitiveDataLogging());

        // add identity
        services.AddIdentity<User, IdentityRole<Guid>>()
           .AddEntityFrameworkStores<RentCarX_DbContext>()
           .AddDefaultTokenProviders();

        services.AddScoped<ICarRepository, CarRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IRentCarX_DbContext, RentCarX_DbContext>();

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContextService, UserContextService>();

        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        services.AddScoped<IStripeProductService, StripeProductService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<InvoiceService>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<ITokenBlacklistService, TokenBlackListService>();

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
