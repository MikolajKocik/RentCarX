using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentCarX.Application.Interfaces.PasswordHasher;
using RentCarX.Application.Services.User;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Interfaces.Services.Stripe;
using RentCarX.Domain.Interfaces.UserContext;
using RentCarX.Domain.Models;
using RentCarX.Infrastructure.Data;
using RentCarX.Infrastructure.Repositories;
using RentCarX.Infrastructure.Services;

namespace RentCarX.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // add connection string from environment variable

            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                ?? throw new Exception("DB_CONNECTION_STRING environment variable not set");

            services.AddDbContext<RentCarX_DbContext>(options =>
              options
                 .UseSqlServer(connectionString)
                 .EnableSensitiveDataLogging());

            // add identity
            services.AddIdentity<User, IdentityRole<Guid>>() 
               .AddEntityFrameworkStores<RentCarX_DbContext>()
               .AddDefaultTokenProviders();


            services.AddScoped<ICarRepository, CarRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddHttpContextAccessor();
            services.AddScoped<IUserContextService, UserContextService>();

            services.AddScoped<IPasswordHasher, PasswordHasher>();

            services.AddScoped<IPaymentService, PaymentService>();
        }
    }
}
