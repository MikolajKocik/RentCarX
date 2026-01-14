using FluentAssertions;
using MediatR;
using Moq;
using RentCarX.Application.CQRS.Commands.Reservation.DeleteReservation;
using RentCarX.Domain.ExceptionModels;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Tests.UnitTests.CQRS.Commands.Reservation.DeleteReservation;

public class SoftDeleteReservationCommandHandlerTests
{
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly SoftDeleteReservationCommandHandler _handler;

    public SoftDeleteReservationCommandHandlerTests()
    {
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _handler = new SoftDeleteReservationCommandHandler(_reservationRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldMarkReservationAsDeleted()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var reservation = new Domain.Models.Reservation
        {
            Id = reservationId,
            IsDeleted = false,
            Car = null
        };

        var command = new SoftDeleteReservationCommand(reservationId);

        _reservationRepositoryMock
            .Setup(x => x.GetReservationByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _reservationRepositoryMock
            .Setup(x => x.SaveToDatabase(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        reservation.IsDeleted.Should().BeTrue();
        _reservationRepositoryMock.Verify(
            x => x.SaveToDatabase(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenReservationHasAssociatedCar_ShouldSetCarAsAvailable()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var car = new Domain.Models.Car
        {
            Id = Guid.NewGuid(),
            IsAvailableFlag = 0
        };

        var reservation = new Domain.Models.Reservation
        {
            Id = reservationId,
            IsDeleted = false,
            Car = car
        };

        var command = new SoftDeleteReservationCommand(reservationId);

        _reservationRepositoryMock
            .Setup(x => x.GetReservationByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _reservationRepositoryMock
            .Setup(x => x.SaveToDatabase(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        car.IsAvailableFlag.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenReservationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var command = new SoftDeleteReservationCommand(reservationId);

        _reservationRepositoryMock
            .Setup(x => x.GetReservationByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Models.Reservation?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain($"id:{reservationId}");
    }

    [Fact]
    public async Task Handle_WhenReservationAlreadyDeleted_ShouldThrowAlreadyDeletedException()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var reservation = new Domain.Models.Reservation
        {
            Id = reservationId,
            IsDeleted = true
        };

        var command = new SoftDeleteReservationCommand(reservationId);

        _reservationRepositoryMock
            .Setup(x => x.GetReservationByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _reservationRepositoryMock
            .Setup(x => x.GetDeletedReservations(reservationId))
            .Returns(new[] { reservation }.AsQueryable());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AlreadyDeletedException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("already deleted");
    }

    [Fact]
    public async Task Handle_WithNullCar_ShouldNotThrowException()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var reservation = new Domain.Models.Reservation
        {
            Id = reservationId,
            IsDeleted = false,
            Car = null
        };

        var command = new SoftDeleteReservationCommand(reservationId);

        _reservationRepositoryMock
            .Setup(x => x.GetReservationByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _reservationRepositoryMock
            .Setup(x => x.SaveToDatabase(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var exception = await Record.ExceptionAsync(
            () => _handler.Handle(command, CancellationToken.None));

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldCallSaveToDatabase()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var reservation = new Domain.Models.Reservation
        {
            Id = reservationId,
            IsDeleted = false,
            Car = null
        };

        var command = new SoftDeleteReservationCommand(reservationId);

        _reservationRepositoryMock
            .Setup(x => x.GetReservationByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _reservationRepositoryMock
            .Setup(x => x.SaveToDatabase(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _reservationRepositoryMock.Verify(
            x => x.SaveToDatabase(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}