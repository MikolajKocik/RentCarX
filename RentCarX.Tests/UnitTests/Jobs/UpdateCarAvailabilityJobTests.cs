using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Models;
using RentCarX.HangfireWorker.Jobs;

namespace RentCarX.Tests.UnitTests.Jobs;

public class UpdateCarAvailabilityJobTests
{
    private readonly Mock<ICarRepository> _carRepositoryMock;
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Mock<ILogger<UpdateCarAvailabilityJob>> _loggerMock;
    private readonly UpdateCarAvailabilityJob _job;

    public UpdateCarAvailabilityJobTests()
    {
        _carRepositoryMock = new Mock<ICarRepository>();
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _loggerMock = new Mock<ILogger<UpdateCarAvailabilityJob>>();
        _job = new UpdateCarAvailabilityJob(
            _carRepositoryMock.Object,
            _reservationRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task PerformJobAsync_WhenUnavailableCarsHaveNoActiveReservations_ShouldMakeThemAvailable()
    {
        // Arrange
        var carIdWithActiveReservation = Guid.NewGuid();
        var carIdToMakeAvailable = Guid.NewGuid();

        var carIdsWithActiveReservations = new List<Guid> { carIdWithActiveReservation };
        var unavailableCars = new List<Car>
        {
            new() { Id = carIdWithActiveReservation },
            new() { Id = carIdToMakeAvailable }
        };

        _reservationRepositoryMock
            .Setup(r => r.GetCarIdsWithActiveReservationAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(carIdsWithActiveReservations);

        _carRepositoryMock
            .Setup(c => c.GetUnavailableCarsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(unavailableCars);

        List<Guid> updatedCarIds = null!;
        _carRepositoryMock
            .Setup(c => c.UpdateAvailabilityForCarsAsync(It.IsAny<IEnumerable<Guid>>(), true, It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Guid>, bool, CancellationToken>((ids, _, _) => updatedCarIds = ids.ToList())
            .Returns(Task.CompletedTask);

        // Act
        await _job.PerformJobAsync(CancellationToken.None);

        // Assert
        _carRepositoryMock.Verify(
            c => c.UpdateAvailabilityForCarsAsync(It.IsAny<IEnumerable<Guid>>(), true, It.IsAny<CancellationToken>()),
            Times.Once);

        updatedCarIds.Should().NotBeNull();
        updatedCarIds.Should().HaveCount(1);
        updatedCarIds.Should().Contain(carIdToMakeAvailable);
    }

    [Fact]
    public async Task PerformJobAsync_WhenAllUnavailableCarsHaveActiveReservations_ShouldNotUpdateAnyCar()
    {
        // Arrange
        var carId1 = Guid.NewGuid();
        var carId2 = Guid.NewGuid();

        var carIdsWithActiveReservations = new List<Guid> { carId1, carId2 };
        var unavailableCars = new List<Car>
        {
            new() { Id = carId1 },
            new() { Id = carId2 }
        };

        _reservationRepositoryMock
            .Setup(r => r.GetCarIdsWithActiveReservationAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(carIdsWithActiveReservations);

        _carRepositoryMock
            .Setup(c => c.GetUnavailableCarsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(unavailableCars);

        // Act
        await _job.PerformJobAsync(CancellationToken.None);

        // Assert
        _carRepositoryMock.Verify(
            c => c.UpdateAvailabilityForCarsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PerformJobAsync_WhenThereAreNoUnavailableCars_ShouldNotUpdateAnyCar()
    {
        // Arrange
        var carIdsWithActiveReservations = new List<Guid> { Guid.NewGuid() };
        var unavailableCars = new List<Car>();

        _reservationRepositoryMock
            .Setup(r => r.GetCarIdsWithActiveReservationAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(carIdsWithActiveReservations);

        _carRepositoryMock
            .Setup(c => c.GetUnavailableCarsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(unavailableCars);

        // Act
        await _job.PerformJobAsync(CancellationToken.None);

        // Assert
        _carRepositoryMock.Verify(
            c => c.UpdateAvailabilityForCarsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PerformJobAsync_WhenNoCarsAreToMakeAvailable_ShouldNotCallUpdate()
    {
        // Arrange
        var carIdsWithActiveReservations = new List<Guid> { Guid.NewGuid() };
        var unavailableCars = new List<Car> { new() { Id = carIdsWithActiveReservations.First() } };

        _reservationRepositoryMock
            .Setup(r => r.GetCarIdsWithActiveReservationAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(carIdsWithActiveReservations);

        _carRepositoryMock
            .Setup(c => c.GetUnavailableCarsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(unavailableCars);

        // Act
        await _job.PerformJobAsync(CancellationToken.None);

        // Assert
        _carRepositoryMock.Verify(
            c => c.UpdateAvailabilityForCarsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}