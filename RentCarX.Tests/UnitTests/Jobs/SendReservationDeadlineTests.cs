using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RentCarX.Application.Helpers;
using RentCarX.Application.Interfaces.Services.NotificationStrategy;
using RentCarX.Application.Services.NotificationService.Flags;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Models;
using RentCarX.HangfireWorker.Jobs;
using RentCarX.Tests.UnitTests.HangfireUtils;

namespace RentCarX.Tests.UnitTests.Jobs;

public class SendReservationDeadlineTests
{
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Mock<IOptions<NotificationFeatureFlags>> _flagsMock;
    private readonly Mock<ILogger<UpdateCarAvailabilityJob>> _loggerMock;
    private readonly Mock<INotificationSender> _azureSenderMock;
    private readonly Mock<INotificationSender> _smtpSenderMock;
    private readonly List<INotificationSender> _senders;
    private readonly SendReservationDeadline _job;

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

        _job = new SendReservationDeadline(
            _reservationRepositoryMock.Object,
            _senders,
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
    public async Task PerformJobAsync_WhenUseAzureNotificationsIsTrue_ShouldSendNotificationsViaAzure()
    {
        // Arrange
        _flagsMock.Setup(f => f.Value).Returns(new NotificationFeatureFlags { UseAzureNotifications = true, UseSmtpProtocol = false });
        
        var reservations = new List<Reservation>
        {
            new() { EndDate = DateTime.UtcNow.AddMinutes(30), User = new User { Email = "test1@example.com" } },
            new() { EndDate = DateTime.UtcNow.AddMinutes(30), User = new User { Email = "test2@example.com" } }
        };
        SetupReservations(reservations);

        // Act
        await _job.PerformJobAsync(CancellationToken.None);

        // Assert
        _azureSenderMock.Verify(s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()), Times.Exactly(2));
        _smtpSenderMock.Verify(s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PerformJobAsync_WhenUseSmtpProtocolIsTrue_ShouldSendNotificationsViaSmtp()
    {
        // Arrange
        _flagsMock.Setup(f => f.Value).Returns(new NotificationFeatureFlags { UseAzureNotifications = false, UseSmtpProtocol = true });
        var reservations = new List<Reservation>
        {
            new() { EndDate = DateTime.UtcNow.AddMinutes(30), User = new User { Email = "test1@example.com" } }
        };
        SetupReservations(reservations);

        // Act
        await _job.PerformJobAsync(CancellationToken.None);

        // Assert
        _smtpSenderMock.Verify(s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()), Times.Once);
        _azureSenderMock.Verify(s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PerformJobAsync_WhenBothFlagsAreTrue_ShouldSendNotificationsViaBoth()
    {
        // true
        _flagsMock.Setup(f => f.Value).Returns(new NotificationFeatureFlags { UseAzureNotifications = true, UseSmtpProtocol = true });
        var reservations = new List<Reservation>
        {
            new() { EndDate = DateTime.UtcNow.AddMinutes(30), User = new User { Email = "test1@example.com" } }
        };
        SetupReservations(reservations);

        // Act
        await _job.PerformJobAsync(CancellationToken.None);

        // Assert
        _azureSenderMock.Verify(s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()), Times.Once);
        _smtpSenderMock.Verify(s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task PerformJobAsync_WhenNoReservationsInTimeframe_ShouldNotSendNotifications()
    {
        // Arrange
        _flagsMock.Setup(f => f.Value).Returns(new NotificationFeatureFlags { UseAzureNotifications = true, UseSmtpProtocol = true });
        var reservations = new List<Reservation>
        {
            new() { EndDate = DateTime.UtcNow.AddHours(1), User = new User { Email = "test1@example.com" } } 
        };
        SetupReservations(reservations);

        // Act
        await _job.PerformJobAsync(CancellationToken.None);

        // Assert
        _azureSenderMock.Verify(s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()), Times.Never);
        _smtpSenderMock.Verify(s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PerformJobAsync_WhenReservationUserHasNoEmail_ShouldNotSendNotification()
    {
        // Arrange
        _flagsMock.Setup(f => f.Value).Returns(new NotificationFeatureFlags { UseAzureNotifications = true, UseSmtpProtocol = true });
        var reservations = new List<Reservation>
        {
            new() { EndDate = DateTime.UtcNow.AddMinutes(30), User = new User { Email = null } },
            new() { EndDate = DateTime.UtcNow.AddMinutes(30), User = new User { Email = "" } },
            new() { EndDate = DateTime.UtcNow.AddMinutes(30), User = new User { Email = " " } },
            new() { EndDate = DateTime.UtcNow.AddMinutes(30), User = null }
        };
        SetupReservations(reservations);

        // Act
        await _job.PerformJobAsync(CancellationToken.None);

        // Assert
        _azureSenderMock.Verify(s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()), Times.Never);
        _smtpSenderMock.Verify(s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PerformJobAsync_WhenAzureSenderIsNotAvailable_ShouldNotThrowAndSmtpShouldWork()
    {
        // Arrange
        _flagsMock.Setup(f => f.Value).Returns(new NotificationFeatureFlags { UseAzureNotifications = false, UseSmtpProtocol = true });
       
        var sendersWithoutAzure = new List<INotificationSender> { _smtpSenderMock.Object };
        var jobWithPartialSenders = new SendReservationDeadline(
            _reservationRepositoryMock.Object,
            sendersWithoutAzure,
            _flagsMock.Object,
            _loggerMock.Object);

        var reservations = new List<Reservation>
        {
            new() { EndDate = DateTime.UtcNow.AddMinutes(30), User = new User { Email = "test1@example.com" } }
        };
        SetupReservations(reservations);

        // Act
        Func<Task> act = async () => await jobWithPartialSenders.PerformJobAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        _smtpSenderMock.Verify(s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()), Times.Once);
        _azureSenderMock.Verify(s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()), Times.Never);
    }
}