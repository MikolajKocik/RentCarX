using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MediatR;
using RentCarX.Application.CQRS.Commands.Car.EditCar;
using RentCarX.Application.Interfaces.Services.File;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Tests.UnitTests.CQRS.Commands.Car;

public class EditCarCommandHandlerTests
{
    private readonly Mock<ICarRepository> _carRepositoryMock;
    private readonly Mock<IFileUploadService> _fileUploadServiceMock;
    private readonly Mock<ILogger<EditCarCommandHandler>> _loggerMock;
    private readonly EditCarCommandHandler _handler;

    public EditCarCommandHandlerTests()
    {
        _carRepositoryMock = new Mock<ICarRepository>();
        _fileUploadServiceMock = new Mock<IFileUploadService>();
        _loggerMock = new Mock<ILogger<EditCarCommandHandler>>();
        
        _handler = new EditCarCommandHandler(
            _carRepositoryMock.Object,
            _fileUploadServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateCarProperties()
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
            IsAvailable = true
        };

        var command = new EditCarCommand
        {
            Id = carId,
            Brand = "Honda",
            Model = "Civic",
            Year = 2024,
            FuelType = "Hybrid",
            PricePerDay = 65.00m,
            IsAvailable = false,
            Image = null
        };

        _carRepositoryMock
            .Setup(x => x.GetCarByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCar);

        _carRepositoryMock
            .Setup(x => x.UpdateCarAsync(It.IsAny<Domain.Models.Car>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _carRepositoryMock.Verify(
            x => x.UpdateCarAsync(It.Is<Domain.Models.Car>(c =>
                c.Brand == "Honda" &&
                c.Model == "Civic" &&
                c.Year == 2024 &&
                c.FuelType == "Hybrid" &&
                c.PricePerDay == 65.00m &&
                c.IsAvailable == false),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCarNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var command = new EditCarCommand
        {
            Id = carId,
            Brand = "Toyota",
            Model = "Camry",
            Year = 2023,
            FuelType = "Petrol",
            PricePerDay = 75.00m,
            IsAvailable = true,
            Image = null
        };

        _carRepositoryMock
            .Setup(x => x.GetCarByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Models.Car?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        _carRepositoryMock.Verify(
            x => x.UpdateCarAsync(It.IsAny<Domain.Models.Car>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenImageIsProvided_ShouldReplaceImage()
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
            ImageUrl = "https://storage.example.com/cars/old-image.jpg"
        };

        var mockFile = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("new-image.jpg");

        var command = new EditCarCommand
        {
            Id = carId,
            Brand = "Toyota",
            Model = "Camry",
            Year = 2020,
            FuelType = "Petrol",
            PricePerDay = 50.00m,
            IsAvailable = true,
            Image = mockFile.Object
        };

        var newImageUrl = "https://storage.example.com/cars/new-image.jpg";

        _carRepositoryMock
            .Setup(x => x.GetCarByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCar);

        _fileUploadServiceMock
            .Setup(x => x.DeleteImageAsync("https://storage.example.com/cars/old-image.jpg", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _fileUploadServiceMock
            .Setup(x => x.UploadImageAsync(mockFile.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newImageUrl);

        _carRepositoryMock
            .Setup(x => x.UpdateCarAsync(It.IsAny<Domain.Models.Car>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _fileUploadServiceMock.Verify(
            x => x.DeleteImageAsync("https://storage.example.com/cars/old-image.jpg", It.IsAny<CancellationToken>()),
            Times.Once);
        _fileUploadServiceMock.Verify(
            x => x.UploadImageAsync(mockFile.Object, It.IsAny<CancellationToken>()),
            Times.Once);
        _carRepositoryMock.Verify(
            x => x.UpdateCarAsync(It.Is<Domain.Models.Car>(c => c.ImageUrl == newImageUrl), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenImageUploadFails_ShouldThrowException()
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
            ImageUrl = "https://storage.example.com/cars/old-image.jpg"
        };

        var mockFile = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("new-image.jpg");

        var command = new EditCarCommand
        {
            Id = carId,
            Brand = "Toyota",
            Model = "Camry",
            Year = 2020,
            FuelType = "Petrol",
            PricePerDay = 50.00m,
            IsAvailable = true,
            Image = mockFile.Object
        };

        _carRepositoryMock
            .Setup(x => x.GetCarByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCar);

        _fileUploadServiceMock
            .Setup(x => x.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _fileUploadServiceMock
            .Setup(x => x.UploadImageAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Upload failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        _carRepositoryMock.Verify(
            x => x.UpdateCarAsync(It.IsAny<Domain.Models.Car>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenImageIsNotProvided_ShouldNotUpdateImage()
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
            ImageUrl = "https://storage.example.com/cars/old-image.jpg"
        };

        var command = new EditCarCommand
        {
            Id = carId,
            Brand = "Honda",
            Model = "Civic",
            Year = 2024,
            FuelType = "Hybrid",
            PricePerDay = 65.00m,
            IsAvailable = false,
            Image = null
        };

        _carRepositoryMock
            .Setup(x => x.GetCarByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCar);

        _carRepositoryMock
            .Setup(x => x.UpdateCarAsync(It.IsAny<Domain.Models.Car>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _fileUploadServiceMock.Verify(
            x => x.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _fileUploadServiceMock.Verify(
            x => x.UploadImageAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _carRepositoryMock.Verify(
            x => x.UpdateCarAsync(It.Is<Domain.Models.Car>(c => c.ImageUrl == "https://storage.example.com/cars/old-image.jpg"), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}