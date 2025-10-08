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
using RentCarX.Infrastructure.Repositories;
using RentCarX.Infrastructure.Services.JWT;
using RentCarX.Infrastructure.Services.Stripe;
using RentCarX.Infrastructure.Settings;
using Stripe;
using Stripe.Checkout;

namespace RentCarX.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment
            )
        {
            if(environment.IsDevelopment())
            {
                var database = configuration.GetSection("Database");
                string server = database["Server"]!;
                string databaseName = database["DatabaseName"]!;
                string username = database["Username"]!;
                string password = database["Password"]!;

                string connectionString = $"Server={server}\\SQLEXPRESS;Database={databaseName};User Id={username};Password={password};";

                services.AddDbContext<RentCarX_DbContext>(options =>
                  options
                     .UseSqlServer(connectionString)
                     .EnableSensitiveDataLogging());
            }

            if(environment.IsProduction())
            {
                // TODO 
                // add azure sql conn 
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

            services.Configure<StripeSettings>(configuration.GetSection("Stripe"));
            //StripeConfiguration.ApiKey = 

            services.AddSingleton(new ProductService()); 
            services.AddSingleton(new PriceService()); 
            services.AddSingleton(new SessionService()); 
        }
    }
}
