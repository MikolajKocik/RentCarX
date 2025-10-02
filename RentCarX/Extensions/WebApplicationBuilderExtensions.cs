using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using MediatR;
using FluentValidation;
using RentCarX.Application.PipelineBehaviors;
using FluentValidation.AspNetCore;
using RentCarX.Application.Services.EmailService;
using RentCarX.Application.MappingsProfile;
using RentCarX.Application.Interfaces.Services.EmailService;

namespace RentCarX.Presentation.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static void AddPresentation(this WebApplicationBuilder builder)
        {
            // SMTP

            builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
            builder.Services.AddTransient<IEmailService, EmailService>();

            // swagger configuration

            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Provide JWT Token without 'Bearer' prefix"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "bearerAuth"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // JWT authentication configuration

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var config = builder.Configuration;

                    Console.WriteLine("\n--- DEBUG: Configuration JWT Validation ---");
                    Console.WriteLine($"DEBUG: Config Jwt:Key (raw): {config["Jwt:Key"]}");
                    Console.WriteLine($"DEBUG: Config Jwt:Issuer: {config["Jwt:Issuer"]}");
                    Console.WriteLine($"DEBUG: Config Jwt:Audience: {config["Jwt:Audience"]}");

                    try
                    {
                        var keyBytes = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);
                        Console.WriteLine($"DEBUG: Config Jwt:Key (Base64 from config): {Convert.ToBase64String(keyBytes)}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"DEBUG: Bad JWT key conversion on bytes/Base64: {ex.Message}");
                    }

                    Console.WriteLine("----------------------------------------");

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = config["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = config["Jwt:Audience"],
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                    };

                    Console.WriteLine("\n--- DEBUG: Final TokenValidationParameters ---");
                    Console.WriteLine($"DEBUG: ValidIssuer: {options.TokenValidationParameters.ValidIssuer}");
                    Console.WriteLine($"DEBUG: ValidAudience: {options.TokenValidationParameters.ValidAudience}");


                    if (options.TokenValidationParameters.IssuerSigningKey is SymmetricSecurityKey symmetricKey)
                    {
                        Console.WriteLine($"DEBUG: IssuerSigningKey (Base64 from SymmetricSecurityKey object): {Convert.ToBase64String(symmetricKey.Key)}");
                    }
                    else
                    {
                        Console.WriteLine("DEBUG: IssuerSigningKey is not type of SymmetricSecurityKey.");
                    }
                    Console.WriteLine($"DEBUG: ValidateLifetime: {options.TokenValidationParameters.ValidateLifetime}");
                    Console.WriteLine($"DEBUG: ClockSkew: {options.TokenValidationParameters.ClockSkew}");
                    Console.WriteLine($"DEBUG: RoleClaimType (Default): {options.TokenValidationParameters.RoleClaimType}"); // Oczekiwany rezultat default ClaimTypes.Role
                    Console.WriteLine("--------------------------------------------");
                });

            builder.Services.AddControllers();

            // Middleware configuration

            builder.Services.AddAuthorization();

            // AutoMapper configuration
            builder.Services.AddAutoMapper(typeof(CarMappingProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(ReservationMappingProfile).Assembly);

            // Serilog configuration
            builder.Host.UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration)
           );

            // MediatR configuration

            builder.Services.AddValidatorsFromAssembly(typeof(AssemblyMarker).Assembly)
                .AddFluentValidationAutoValidation();

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
            });

            var stripeApiKey = builder.Configuration.GetValue<string>("STRIPE_API_KEY");

            if (string.IsNullOrEmpty(stripeApiKey))
            {
                throw new InvalidOperationException("Stripe Secret Key environment variable 'STRIPE_API_KEY' is not set.");
            }
        }
    }
}
