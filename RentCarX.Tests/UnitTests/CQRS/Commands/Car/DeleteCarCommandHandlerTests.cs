using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MediatR;
using RentCarX.Application.CQRS.Commands.Car.DeleteCar;
using RentCarX.Application.Interfaces.Services.File;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.Repositories;
using Xunit;

namespace RentCarX.Tests.UnitTests.CQRS.Commands.Car;

public class DeleteCarCommandHandlerTests
{
    private readonly Mock<ICarRepository> _carRepositoryMock;
    private readonly Mock<IFileUploadService> _fileUploadServiceMock;
    private readonly Mock<ILogger<DeleteCarCommandHandler>> _loggerMock;
    private readonly DeleteCarCommandHandler _handler;

    public DeleteCarCommandHandlerTests()
    {
        _carRepositoryMock = new Mock<ICarRepository>();
        _fileUploadServiceMock = new Mock<IFileUploadService>();
        _loggerMock = new Mock<ILogger<DeleteCarCommandHandler>>();
        
        _handler = new DeleteCarCommandHandler(
            _carRepositoryMock.Object,
            _fileUploadServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldDeleteCar()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var existingCar = new Domain.Models.Car
        {
            Id = carId,
            Brand = "Toyota",
            Model = "Camry",
            Year = 2020,
            FuelType = "Petrol",
            PricePerDay = 50.00m,
            IsAvailable = true,
            ImageUrl = null
        };

        var command = new DeleteCarCommand { Id = carId };

        _carRepositoryMock
            .Setup(x => x.GetCarByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCar);

        _carRepositoryMock
            .Setup(x => x.RemoveAsync(carId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _carRepositoryMock.Verify(
            x => x.RemoveAsync(carId, It.IsAny<CancellationToken>()),
            Times.Once);
        _fileUploadServiceMock.Verify(
            x => x.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCarHasImage_ShouldDeleteImageBeforeRemovingCar()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var imageUrl = "/images/cars/test-image.jpg";
        var existingCar = new Domain.Models.Car
        {
            Id = carId,
            Brand = "Honda",
            Model = "Civic",
            Year = 2024,
            FuelType = "Hybrid",
            PricePerDay = 65.00m,
            IsAvailable = true,
            ImageUrl = imageUrl
        };

        var command = new DeleteCarCommand { Id = carId };

        _carRepositoryMock
            .Setup(x => x.GetCarByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCar);

        _fileUploadServiceMock
            .Setup(x => x.DeleteImageAsync(imageUrl, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _carRepositoryMock
            .Setup(x => x.RemoveAsync(carId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _fileUploadServiceMock.Verify(
            x => x.DeleteImageAsync(imageUrl, It.IsAny<CancellationToken>()),
            Times.Once);
        _carRepositoryMock.Verify(
            x => x.RemoveAsync(carId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCarNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var command = new DeleteCarCommand { Id = carId };

        _carRepositoryMock
            .Setup(x => x.GetCarByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Models.Car?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        _carRepositoryMock.Verify(
            x => x.RemoveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenImageDeleteFails_ShouldStillDeleteCar()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var imageUrl = "/images/cars/test-image.jpg";
        var existingCar = new Domain.Models.Car
        {
            Id = carId,
            Brand = "BMW",
            Model = "X5",
            Year = 2023,
            FuelType = "Diesel",
            PricePerDay = 120.00m,
            IsAvailable = true,
            ImageUrl = imageUrl
        };

        var command = new DeleteCarCommand { Id = carId };

        _carRepositoryMock
            .Setup(x => x.GetCarByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCar);

        _fileUploadServiceMock
            .Setup(x => x.DeleteImageAsync(imageUrl, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Delete image failed"));

        _carRepositoryMock
            .Setup(x => x.RemoveAsync(carId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _carRepositoryMock.Verify(
            x => x.RemoveAsync(carId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRequestIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
    }
}