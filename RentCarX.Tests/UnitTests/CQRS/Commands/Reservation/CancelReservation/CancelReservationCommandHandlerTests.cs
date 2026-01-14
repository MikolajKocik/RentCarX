using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using RentCarX.Application.CQRS.Commands.Reservation.CancelReservation;
using RentCarX.Domain.ExceptionModels;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.DbContext;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Interfaces.Services.Stripe;
using RentCarX.Domain.Interfaces.UserContext;
using RentCarX.Domain.Models;
using RentCarX.Domain.Models.Enums;
using RentCarX.Domain.Models.Stripe;
using Xunit;

namespace RentCarX.Tests.UnitTests.CQRS.Commands.Reservation.CancelReservation;

public class CancelReservationCommandHandlerTests
{
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Mock<IRentCarX_DbContext> _contextMock;
    private readonly Mock<ILogger<CancelReservationCommandHandler>> _loggerMock;
    private readonly Mock<IUserContextService> _userServiceMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly CancelReservationCommandHandler _handler;

    public CancelReservationCommandHandlerTests()
    {
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _contextMock = new Mock<IRentCarX_DbContext>();
        _loggerMock = new Mock<ILogger<CancelReservationCommandHandler>>();
        _userServiceMock = new Mock<IUserContextService>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _paymentServiceMock = new Mock<IPaymentService>();

        _handler = new CancelReservationCommandHandler(
            _reservationRepositoryMock.Object,
            _contextMock.Object,
            _loggerMock.Object,
            _userServiceMock.Object,
            _paymentRepositoryMock.Object,
            _paymentServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldCancelReservation()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(5);

        var reservation = new RentCarX.Domain.Models.Reservation
        {
            Id = reservationId,
            UserId = userId,
            StartDate = startDate,
            Status = ReservationStatus.Confirmed,
            IsDeleted = false,
            IsPaid = true,
            Car = null!
        };

        var command = new CancelReservationCommand(reservationId);
        var mockTransaction = new Mock<IDbContextTransaction>();

        _reservationRepositoryMock
            .Setup(x => x.GetReservationByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _userServiceMock.Setup(x => x.UserId).Returns(userId);

        _paymentRepositoryMock
            .Setup(x => x.GetByReservationId(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        _contextMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        _reservationRepositoryMock
            .Setup(x => x.SaveToDatabase(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
        reservation.IsDeleted.Should().BeTrue();
        reservation.IsPaid.Should().BeFalse();
        mockTransaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidId_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new CancelReservationCommand(Guid.Empty);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("invalid");
    }

    [Fact]
    public async Task Handle_WhenReservationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var command = new CancelReservationCommand(reservationId);

        _reservationRepositoryMock
            .Setup(x => x.GetReservationByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RentCarX.Domain.Models.Reservation?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_WhenUserNotOwner_ShouldThrowForbiddenException()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(5);

        var reservation = new RentCarX.Domain.Models.Reservation
        {
            Id = reservationId,
            UserId = ownerId,
            StartDate = startDate,
            Status = ReservationStatus.Pending,
            IsDeleted = false,
            IsPaid = false,
            Car = null!
        };

        var command = new CancelReservationCommand(reservationId);

        _reservationRepositoryMock
            .Setup(x => x.GetReservationByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _userServiceMock.Setup(x => x.UserId).Returns(otherUserId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("don't have permission");
    }

    [Fact]
    public async Task Handle_WhenReservationStarted_ShouldThrowBadRequestException()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddHours(-1);

        var reservation = new RentCarX.Domain.Models.Reservation
        {
            Id = reservationId,
            UserId = userId,
            StartDate = startDate,
            Status = ReservationStatus.Pending,
            IsDeleted = false,
            IsPaid = false,
            Car = null!
        };

        var command = new CancelReservationCommand(reservationId);

        _reservationRepositoryMock
            .Setup(x => x.GetReservationByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _userServiceMock.Setup(x => x.UserId).Returns(userId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("already started");
    }

    [Fact]
    public async Task Handle_WithPayment_ShouldCreateRefund()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(5);
        var paymentIntentId = "pi_test123";

        var reservation = new RentCarX.Domain.Models.Reservation
        {
            Id = reservationId,
            UserId = userId,
            StartDate = startDate,
            Status = ReservationStatus.Confirmed,
            IsDeleted = false,
            IsPaid = true,
            Car = null!
        };

        var payment = new Payment
        {
            Id = 1,
            ReservationId = reservationId,
            StripePaymentIntentId = paymentIntentId,
            Status = PaymentStatus.Succeeded
        };

        var command = new CancelReservationCommand(reservationId);
        var mockTransaction = new Mock<IDbContextTransaction>();

        _reservationRepositoryMock
            .Setup(x => x.GetReservationByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _userServiceMock.Setup(x => x.UserId).Returns(userId);

        _paymentRepositoryMock
            .Setup(x => x.GetByReservationId(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _paymentServiceMock
            .Setup(x => x.CreateRefundAsync(paymentIntentId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _contextMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        _reservationRepositoryMock
            .Setup(x => x.SaveToDatabase(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.RefundedAt.Should().NotBeNull();
        _paymentServiceMock.Verify(
            x => x.CreateRefundAsync(paymentIntentId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPaymentHasNoStripeId_ShouldNotCreateRefund()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(5);

        var reservation = new RentCarX.Domain.Models.Reservation
        {
            Id = reservationId,
            UserId = userId,
            StartDate = startDate,
            Status = ReservationStatus.Confirmed,
            IsDeleted = false,
            IsPaid = true,
            Car = null!
        };

        var payment = new Payment
        {
            Id = 1,
            ReservationId = reservationId,
            StripePaymentIntentId = string.Empty,
            Status = PaymentStatus.Succeeded
        };

        var command = new CancelReservationCommand(reservationId);
        var mockTransaction = new Mock<IDbContextTransaction>();

        _reservationRepositoryMock
            .Setup(x => x.GetReservationByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _userServiceMock.Setup(x => x.UserId).Returns(userId);

        _paymentRepositoryMock
            .Setup(x => x.GetByReservationId(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _contextMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        _reservationRepositoryMock
            .Setup(x => x.SaveToDatabase(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _paymentServiceMock.Verify(
            x => x.CreateRefundAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCarExists_ShouldSetCarAsAvailable()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(5);

        var car = new RentCarX.Domain.Models.Car
        {
            Id = Guid.NewGuid(),
            IsAvailableFlag = 0
        };

        var reservation = new RentCarX.Domain.Models.Reservation
        {
            Id = reservationId,
            UserId = userId,
            StartDate = startDate,
            Status = ReservationStatus.Confirmed,
            IsDeleted = false,
            IsPaid = true,
            Car = car
        };

        var command = new CancelReservationCommand(reservationId);
        var mockTransaction = new Mock<IDbContextTransaction>();

        _reservationRepositoryMock
            .Setup(x => x.GetReservationByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _userServiceMock.Setup(x => x.UserId).Returns(userId);

        _paymentRepositoryMock
            .Setup(x => x.GetByReservationId(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        _contextMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        _reservationRepositoryMock
            .Setup(x => x.SaveToDatabase(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        car.IsAvailableFlag.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldRollbackTransaction()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(5);

        var reservation = new RentCarX.Domain.Models.Reservation
        {
            Id = reservationId,
            UserId = userId,
            StartDate = startDate,
            Status = ReservationStatus.Pending,
            IsDeleted = false,
            IsPaid = false,
            Car = null!
        };

        var command = new CancelReservationCommand(reservationId);
        var mockTransaction = new Mock<IDbContextTransaction>();

        _reservationRepositoryMock
            .Setup(x => x.GetReservationByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _userServiceMock.Setup(x => x.UserId).Returns(userId);

        _paymentRepositoryMock
            .Setup(x => x.GetByReservationId(reservationId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        _contextMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(
            () => _handler.Handle(command, CancellationToken.None));

        mockTransaction.Verify(
            x => x.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}