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
using Asp.Versioning.ApiExplorer;
using Microsoft.FeatureManagement;

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
}
