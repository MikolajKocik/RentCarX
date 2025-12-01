using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.FeatureManagement;
using RentCarX.Application.Interfaces.Services.Hangfire;
using RentCarX.Application.Interfaces.Services.Notifications;
using RentCarX.Application.Interfaces.Services.NotificationStrategy;
using RentCarX.Application.Jobs;
using RentCarX.Application.MappingsProfile;
using RentCarX.Application.PipelineBehaviors;
using RentCarX.Application.Services.NotificationService;
using RentCarX.Application.Services.NotificationService.Flags;
using RentCarX.Application.Services.NotificationService.Settings;
using RentCarX.HangfireWorker.Common;
using RentCarX.Presentation.Extensions.JWT;
using RentCarX.Presentation.Extensions.Swagger;
using Serilog;
using System.Collections.Concurrent;

namespace RentCarX.Presentation.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddPresentation(this WebApplicationBuilder builder)
    {
        // parallel queue for reservation background job
        builder.Services.AddSingleton(new ConcurrentQueue<Guid>());

        builder.Services.AddSingleton<ReservationQueueWorker>();
        builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<ReservationQueueWorker>());

        // hangfire job scheduler
        builder.Services.AddSingleton<IJobScheduler, HangfireJobScheduler>();

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

        // notification register device -> web push
        builder.Services.AddTransient<INotificationRegister, NotificationRegistrationService>();

        // swagger configuration
        SwaggerExtend.AddSwaggerImplementation(builder);
       
        // JWT authentication configuration
        JsonWebTokenExtend.SetToken(builder);

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
            throw new InvalidOperationException();
        }
    }
}
