using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RentCarX.Application.CQRS.Commands.Reservation.CreateReservation;
using RentCarX.Application.Helpers;
using RentCarX.Application.Interfaces.Services.Hangfire;
using RentCarX.Application.Interfaces.Services.NotificationStrategy;
using RentCarX.Application.Jobs;
using RentCarX.Application.Services.NotificationService.Flags;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.DbContext;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Interfaces.UserContext;
using Xunit;

namespace RentCarX.Tests.UnitTests.CQRS.Commands.Reservation.CreateReservation;

public class CreateReservationCommandHandlerTests
{
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Mock<ICarRepository> _carRepositoryMock;
    private readonly Mock<IUserContextService> _userContextMock;
    private readonly Mock<IEnumerable<INotificationSender>> _sendersMock;
    private readonly Mock<IJobScheduler> _jobSchedulerMock;
    private readonly Mock<IRentCarX_DbContext> _contextMock;
    private readonly ReservationQueueWorker _reservationQueue;
    private readonly Mock<ILogger<CreateReservationCommandHandler>> _loggerMock;
    private readonly CreateReservationCommandHandler _handler;
    private readonly NotificationFeatureFlags _flags;

    public CreateReservationCommandHandlerTests()
    {
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _carRepositoryMock = new Mock<ICarRepository>();
        _userContextMock = new Mock<IUserContextService>();
        _sendersMock = new Mock<IEnumerable<INotificationSender>>();
        _jobSchedulerMock = new Mock<IJobScheduler>();
        _contextMock = new Mock<IRentCarX_DbContext>();
        _loggerMock = new Mock<ILogger<CreateReservationCommandHandler>>();

        var queue = new System.Collections.Concurrent.ConcurrentQueue<Guid>();
        _reservationQueue = new ReservationQueueWorker(queue);

        _flags = new NotificationFeatureFlags
        {
            UseSmtpProtocol = true,
            UseAzureNotifications = false
        };

        var optionsMock = Options.Create(_flags);

        _handler = new CreateReservationCommandHandler(
            _reservationRepositoryMock.Object,
            _carRepositoryMock.Object,
            _userContextMock.Object,
            _sendersMock.Object,
            optionsMock,
            _jobSchedulerMock.Object,
            _contextMock.Object,
            _reservationQueue,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateReservation()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = startDate.AddDays(5);

        var command = new CreateReservationCommand
        {
            CarId = carId,
            StartDate = startDate,
            EndDate = endDate
        };

        var car = new Domain.Models.Car
        {
            Id = carId,
            Brand = "Toyota",
            Model = "Camry",
            PricePerDay = 100m,
            IsAvailableFlag = 1
        };

        var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();

        _carRepositoryMock
            .Setup(x => x.GetCarByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);

        _contextMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        _reservationRepositoryMock
            .Setup(x => x.HasOverlappingReservationAsync(carId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _reservationRepositoryMock
            .Setup(x => x.Create(It.IsAny<Domain.Models.Reservation>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userContextMock.Setup(x => x.Email).Returns("user@example.com");

        var mockNotificationSender = new Mock<INotificationSender>();
        _sendersMock
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { mockNotificationSender.Object }.AsEnumerable().GetEnumerator());

        mockNotificationSender
            .Setup(x => x.StrategyName)
            .Returns(NotificationStrategyOptions.Smtp);

        mockNotificationSender
            .Setup(x => x.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _reservationRepositoryMock.Verify(
            x => x.Create(It.IsAny<Domain.Models.Reservation>(), It.IsAny<CancellationToken>()),
            Times.Once);
        mockTransaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCarNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var command = new CreateReservationCommand
        {
            CarId = carId,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(6)
        };

        _carRepositoryMock
            .Setup(x => x.GetCarByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Models.Car?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Car not found");
    }

    [Fact]
    public async Task Handle_WhenCarNotAvailable_ShouldThrowBadRequestException()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var command = new CreateReservationCommand
        {
            CarId = carId,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(6)
        };

        var car = new Domain.Models.Car
        {
            Id = carId,
            IsAvailableFlag = 0
        };

        var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();

        _carRepositoryMock
            .Setup(x => x.GetCarByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);

        _contextMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("not available");
    }

    [Fact]
    public async Task Handle_WhenOverlappingReservationExists_ShouldThrowConflictException()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = startDate.AddDays(5);

        var command = new CreateReservationCommand
        {
            CarId = carId,
            StartDate = startDate,
            EndDate = endDate
        };

        var car = new Domain.Models.Car
        {
            Id = carId,
            IsAvailableFlag = 1
        };

        var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();

        _carRepositoryMock
            .Setup(x => x.GetCarByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);

        _contextMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        _reservationRepositoryMock
            .Setup(x => x.HasOverlappingReservationAsync(carId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("already reserved");
        mockTransaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCalculateTotalCostCorrectly()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = startDate.AddDays(5);
        const decimal pricePerDay = 100m;

        var command = new CreateReservationCommand
        {
            CarId = carId,
            StartDate = startDate,
            EndDate = endDate
        };

        var car = new Domain.Models.Car
        {
            Id = carId,
            Brand = "Toyota",
            Model = "Camry",
            PricePerDay = pricePerDay,
            IsAvailableFlag = 1
        };

        var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
        Domain.Models.Reservation? capturedReservation = null;

        _carRepositoryMock
            .Setup(x => x.GetCarByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);

        _contextMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        _reservationRepositoryMock
            .Setup(x => x.HasOverlappingReservationAsync(carId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _reservationRepositoryMock
            .Setup(x => x.Create(It.IsAny<Domain.Models.Reservation>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Models.Reservation, CancellationToken>((res, _) => capturedReservation = res)
            .Returns(Task.CompletedTask);

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userContextMock.Setup(x => x.Email).Returns("user@example.com");

        var mockNotificationSender = new Mock<INotificationSender>();
        _sendersMock
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { mockNotificationSender.Object }.AsEnumerable().GetEnumerator());

        mockNotificationSender
            .Setup(x => x.StrategyName)
            .Returns(NotificationStrategyOptions.Smtp);

        mockNotificationSender
            .Setup(x => x.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedReservation.Should().NotBeNull();
        capturedReservation!.TotalCost.Should().Be(500m);
    }

    [Fact]
    public async Task Handle_ShouldSendNotificationWhenFlagEnabled()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var userEmail = "user@example.com";
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = startDate.AddDays(5);

        var command = new CreateReservationCommand
        {
            CarId = carId,
            StartDate = startDate,
            EndDate = endDate
        };

        var car = new Domain.Models.Car
        {
            Id = carId,
            Brand = "Toyota",
            Model = "Camry",
            PricePerDay = 100m,
            IsAvailableFlag = 1
        };

        var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();

        _carRepositoryMock
            .Setup(x => x.GetCarByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);

        _contextMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        _reservationRepositoryMock
            .Setup(x => x.HasOverlappingReservationAsync(carId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _reservationRepositoryMock
            .Setup(x => x.Create(It.IsAny<Domain.Models.Reservation>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userContextMock.Setup(x => x.Email).Returns(userEmail);

        var mockNotificationSender = new Mock<INotificationSender>();
        _sendersMock
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { mockNotificationSender.Object }.AsEnumerable().GetEnumerator());

        mockNotificationSender
            .Setup(x => x.StrategyName)
            .Returns(NotificationStrategyOptions.Smtp);

        mockNotificationSender
            .Setup(x => x.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        mockNotificationSender.Verify(
            x => x.SendNotificationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>(),
                userEmail),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldRollbackTransaction()
    {
        // Arrange
        var carId = Guid.NewGuid();

        var command = new CreateReservationCommand
        {
            CarId = carId,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(6)
        };

        var car = new Domain.Models.Car
        {
            Id = carId,
            IsAvailableFlag = 1
        };

        var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();

        _carRepositoryMock
            .Setup(x => x.GetCarByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);

        _contextMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        _reservationRepositoryMock
            .Setup(x => x.HasOverlappingReservationAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(
            () => _handler.Handle(command, CancellationToken.None));

        mockTransaction.Verify(
            x => x.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenReservationEndsInFuture_ShouldScheduleJob()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(5);
        var endDate = startDate.AddDays(5);

        var command = new CreateReservationCommand
        {
            CarId = carId,
            StartDate = startDate,
            EndDate = endDate
        };

        var car = new Domain.Models.Car
        {
            Id = carId,
            Brand = "Toyota",
            Model = "Camry",
            PricePerDay = 100m,
            IsAvailableFlag = 1
        };

        var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();

        _carRepositoryMock
            .Setup(x => x.GetCarByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);

        _contextMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        _reservationRepositoryMock
            .Setup(x => x.HasOverlappingReservationAsync(carId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _reservationRepositoryMock
            .Setup(x => x.Create(It.IsAny<Domain.Models.Reservation>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userContextMock.Setup(x => x.Email).Returns("user@example.com");

        var mockNotificationSender = new Mock<INotificationSender>();
        _sendersMock
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { mockNotificationSender.Object }.AsEnumerable().GetEnumerator());

        mockNotificationSender
            .Setup(x => x.StrategyName)
            .Returns(NotificationStrategyOptions.Smtp);

        mockNotificationSender
            .Setup(x => x.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _jobSchedulerMock.Verify(
            x => x.SetJob(It.IsAny<Domain.Models.Reservation>()),
            Times.Once);
    }
}