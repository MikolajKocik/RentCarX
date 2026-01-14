using FluentAssertions;
using MediatR;
using Moq;
using RentCarX.Application.CQRS.Commands.Reservation.DeleteReservation;
using RentCarX.Domain.ExceptionModels;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Tests.UnitTests.CQRS.Commands.Reservation.DeleteReservation;

public class HardDeleteReservationCommandHandlerTests
{
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly HardDeleteReservationCommandHandler _handler;

    public HardDeleteReservationCommandHandlerTests()
    {
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _handler = new HardDeleteReservationCommandHandler(_reservationRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldDeleteReservation()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var reservation = new Domain.Models.Reservation
        {
            Id = reservationId,
            IsDeleted = false,
            Car = null
        };

        var command = new HardDeleteReservationCommand(reservationId);

        _reservationRepositoryMock
            .Setup(x => x.GetReservationByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _reservationRepositoryMock
            .Setup(x => x.DeleteAsync(reservationId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _reservationRepositoryMock.Verify(
            x => x.DeleteAsync(reservationId, It.IsAny<CancellationToken>()),
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

        var command = new HardDeleteReservationCommand(reservationId);

        _reservationRepositoryMock
            .Setup(x => x.GetReservationByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _reservationRepositoryMock
            .Setup(x => x.DeleteAsync(reservationId, It.IsAny<CancellationToken>()))
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
        var command = new HardDeleteReservationCommand(reservationId);

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

        var command = new HardDeleteReservationCommand(reservationId);

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

        var command = new HardDeleteReservationCommand(reservationId);

        _reservationRepositoryMock
            .Setup(x => x.GetReservationByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _reservationRepositoryMock
            .Setup(x => x.DeleteAsync(reservationId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var exception = await Record.ExceptionAsync(
            () => _handler.Handle(command, CancellationToken.None));

        // Assert
        exception.Should().BeNull();
    }
}