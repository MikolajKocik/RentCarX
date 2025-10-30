using Asp.Versioning.ApiExplorer;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RentCarX.Application.Interfaces.Services.NotificationStrategy;
using RentCarX.Application.MappingsProfile;
using RentCarX.Application.PipelineBehaviors;
using RentCarX.Application.Services.NotificationService;
using RentCarX.Application.Services.NotificationService.Flags;
using RentCarX.Application.Services.NotificationService.Settings;
using Serilog;
using System.Text;

namespace RentCarX.Presentation.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddPresentation(this WebApplicationBuilder builder)
    {
        // notification configs
        builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
        builder.Services.Configure<NotificationHubSettings>(
            builder.Configuration.GetSection("NotificationHub"));

        // feature flags
        builder.Services.Configure<NotificationFeatureFlags>(
            builder.Configuration.GetSection("FeatureManagement"));

        // notification strategy pattern
        builder.Services.AddTransient<INotificationSender, SmtpNotificationSender>();
        builder.Services.AddTransient<INotificationSender, AzureNotificationSender>();

        // swagger configuration
        builder.Services.AddSwaggerGen(c =>
        {
            var apiVersionDescriptionProvider = builder.Services.BuildServiceProvider()
                .GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                c.SwaggerDoc(description.GroupName, new OpenApiInfo
                {
                    Title = $"Api version: {description.ApiVersion}",
                    Version = description.ApiVersion.ToString()
                });
            }

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
            });

        builder.Services.AddControllers();

        // Feature flags management
        builder.Services.AddFeatureManagement();

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

        var stripeApiKey = builder.Configuration.GetSection("Stripe").GetValue<string>("SecretKey");

        if (string.IsNullOrEmpty(stripeApiKey))
        {
            throw new InvalidOperationException("Stripe Secret Key environment variable is not set.");
        }
    }
}
