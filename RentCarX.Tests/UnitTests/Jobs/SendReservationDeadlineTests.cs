using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RentCarX.Application.Helpers;
using RentCarX.Application.Interfaces.Services.NotificationStrategy;
using RentCarX.Application.Services.NotificationService.Flags;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Models;
using RentCarX.Domain.Models.Enums;
using RentCarX.HangfireWorker.Jobs;
using RentCarX.Tests.UnitTests.HangfireUtils;
using Xunit;

namespace RentCarX.Tests.UnitTests.Jobs;

public class SendReservationDeadlineTests
{
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Mock<IOptions<NotificationFeatureFlags>> _flagsMock;
    private readonly Mock<ILogger<UpdateCarAvailabilityJob>> _loggerMock;
    private readonly Mock<INotificationSender> _azureSenderMock;
    private readonly Mock<INotificationSender> _smtpSenderMock;
    private readonly List<INotificationSender> _senders;

    public SendReservationDeadlineTests()
    {
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _flagsMock = new Mock<IOptions<NotificationFeatureFlags>>();
        _loggerMock = new Mock<ILogger<UpdateCarAvailabilityJob>>();
        _azureSenderMock = new Mock<INotificationSender>();
        _smtpSenderMock = new Mock<INotificationSender>();

        _azureSenderMock.Setup(s => s.StrategyName).Returns(NotificationStrategyOptions.Azure);
        _smtpSenderMock.Setup(s => s.StrategyName).Returns(NotificationStrategyOptions.Smtp);

        _senders = new List<INotificationSender> { _azureSenderMock.Object, _smtpSenderMock.Object };

        _flagsMock.Setup(f => f.Value).Returns(new NotificationFeatureFlags());
    }

    private SendReservationDeadline CreateJob(List<INotificationSender>? senders = null)
    {
        return new SendReservationDeadline(
            _reservationRepositoryMock.Object,
            senders ?? _senders,
            _flagsMock.Object,
            _loggerMock.Object);
    }

    private void SetupReservations(List<Reservation> reservations)
    {
        var mockReservations = reservations.AsQueryable();
        var asyncEnumerable = new TestAsyncEnumerable<Reservation>(mockReservations);
        _reservationRepositoryMock.Setup(r => r.GetAll()).Returns(asyncEnumerable);
    }

    [Fact]
    public async Task PerformJobAsync_WhenNoReservationsInTimeframe_ShouldNotSendNotifications()
    {
        // Arrange
        _flagsMock.Setup(f => f.Value).Returns(
            new NotificationFeatureFlags { UseAzureNotifications = true, UseSmtpProtocol = true });
        
        var reservations = new List<Reservation>
        {
            new() 
            { 
                Id = Guid.NewGuid(),
                CarId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EndDate = DateTime.UtcNow.AddHours(1),
                Status = ReservationStatus.Confirmed,
                IsPaid = true,
                User = new User { Email = "test1@example.com" } 
            }
        };
        SetupReservations(reservations);
        
        var job = CreateJob();

        // Act
        await job.PerformJobAsync(CancellationToken.None);

        // Assert
        _azureSenderMock.Verify(
            s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<CancellationToken>(), It.IsAny<string>()), 
            Times.Never);
        _smtpSenderMock.Verify(
            s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<CancellationToken>(), It.IsAny<string>()), 
            Times.Never);
    }

    [Fact]
    public async Task PerformJobAsync_WhenReservationUserHasNoEmail_ShouldNotSendNotification()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var reminderTime = now.AddMinutes(30);
        
        _flagsMock.Setup(f => f.Value).Returns(
            new NotificationFeatureFlags { UseAzureNotifications = true, UseSmtpProtocol = true });
        
        var reservations = new List<Reservation>
        {
            new() 
            { 
                Id = Guid.NewGuid(),
                CarId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EndDate = reminderTime,
                Status = ReservationStatus.Confirmed,
                IsPaid = true,
                User = new User { Email = null } 
            },
            new() 
            { 
                Id = Guid.NewGuid(),
                CarId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EndDate = reminderTime,
                Status = ReservationStatus.Confirmed,
                IsPaid = true,
                User = new User { Email = "" } 
            },
            new() 
            { 
                Id = Guid.NewGuid(),
                CarId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EndDate = reminderTime,
                Status = ReservationStatus.Confirmed,
                IsPaid = true,
                User = new User { Email = " " } 
            },
            new() 
            { 
                Id = Guid.NewGuid(),
                CarId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EndDate = reminderTime,
                Status = ReservationStatus.Confirmed,
                IsPaid = true,
                User = null 
            }
        };
        SetupReservations(reservations);
        
        var job = CreateJob();

        // Act
        await job.PerformJobAsync(CancellationToken.None);

        // Assert
        _azureSenderMock.Verify(
            s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<CancellationToken>(), It.IsAny<string>()), 
            Times.Never);
        _smtpSenderMock.Verify(
            s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<CancellationToken>(), It.IsAny<string>()), 
            Times.Never);
    }

    [Fact]
    public async Task PerformJobAsync_WhenReservationIsCancelled_ShouldNotSendReminder()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var reminderTime = now.AddMinutes(30);
        
        _flagsMock.Setup(f => f.Value).Returns(
            new NotificationFeatureFlags { UseSmtpProtocol = true, UseAzureNotifications = false });

        var reservations = new List<Reservation>
        {
            new() 
            { 
                Id = Guid.NewGuid(),
                CarId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EndDate = reminderTime,
                Status = ReservationStatus.Cancelled,
                IsPaid = true,
                User = new User { Email = "test@example.com" }
            }
        };
        SetupReservations(reservations);
        
        var job = CreateJob();

        // Act
        await job.PerformJobAsync(CancellationToken.None);

        // Assert 
        _smtpSenderMock.Verify(
            s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<CancellationToken>(), It.IsAny<string>()), 
            Times.Never);
    }

    [Fact]
    public async Task PerformJobAsync_WhenReservationNotPaid_ShouldNotSendReminder()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var reminderTime = now.AddMinutes(30);
        
        _flagsMock.Setup(f => f.Value).Returns(
            new NotificationFeatureFlags { UseSmtpProtocol = true, UseAzureNotifications = false });

        var reservations = new List<Reservation>
        {
            new() 
            { 
                Id = Guid.NewGuid(),
                CarId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EndDate = reminderTime,
                Status = ReservationStatus.Confirmed,
                IsPaid = false,
                User = new User { Email = "test@example.com" }
            }
        };
        SetupReservations(reservations);
        
        var job = CreateJob();

        // Act
        await job.PerformJobAsync(CancellationToken.None);

        // Assert 
        _smtpSenderMock.Verify(
            s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<CancellationToken>(), It.IsAny<string>()), 
            Times.Never);
    }
}