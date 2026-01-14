using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RentCarX.Application.CQRS.Commands.Car.AddCar;
using RentCarX.Application.CQRS.Commands.Car.CreateCar;
using RentCarX.Application.Interfaces.Services.File;
using RentCarX.Domain.Interfaces.Repositories;
using Stripe;
using Xunit;

namespace RentCarX.Tests.UnitTests.CQRS.Commands.Car;

public class CreateCarCommandHandlerTests
{
    private readonly Mock<ICarRepository> _carRepositoryMock;
    private readonly Mock<ProductService> _productServiceMock;
    private readonly Mock<IFileUploadService> _fileUploadServiceMock;
    private readonly Mock<ILogger<CreateCarCommandHandler>> _loggerMock;
    private readonly CreateCarCommandHandler _handler;

    public CreateCarCommandHandlerTests()
    {
        _carRepositoryMock = new Mock<ICarRepository>();
        _productServiceMock = new Mock<ProductService>(null!);
        _fileUploadServiceMock = new Mock<IFileUploadService>();
        _loggerMock = new Mock<ILogger<CreateCarCommandHandler>>();
        
        _handler = new CreateCarCommandHandler(
            _carRepositoryMock.Object,
            _productServiceMock.Object,
            _fileUploadServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateCarWithStripeProduct()
    {
        // Arrange
        var command = new CreateCarCommand
        {
            Brand = "Toyota",
            Model = "Camry",
            Year = 2023,
            FuelType = "Petrol",
            PricePerDay = 75.50m,
            IsAvailable = true,
            Image = null
        };

        var stripeProduct = new Product
        {
            Id = "prod_test123",
            DefaultPriceId = "price_test123"
        };

        _productServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<ProductCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stripeProduct);

        _carRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Domain.Models.Car>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _productServiceMock.Verify(
            x => x.CreateAsync(It.IsAny<ProductCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _carRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Domain.Models.Car>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenImageIsProvided_ShouldUploadImage()
    {
        // Arrange
        var mockFile = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.jpg");
        mockFile.Setup(f => f.Length).Returns(1024);

        var command = new CreateCarCommand
        {
            Brand = "Honda",
            Model = "Civic",
            Year = 2024,
            FuelType = "Hybrid",
            PricePerDay = 65.00m,
            IsAvailable = true,
            Image = mockFile.Object
        };

        var stripeProduct = new Product
        {
            Id = "prod_test123",
            DefaultPriceId = "price_test123"
        };

        var imageUrl = "https://storage.example.com/cars/test-image.jpg";

        _productServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<ProductCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stripeProduct);

        _fileUploadServiceMock
            .Setup(x => x.UploadImageAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(imageUrl);

        _carRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Domain.Models.Car>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _fileUploadServiceMock.Verify(
            x => x.UploadImageAsync(mockFile.Object, It.IsAny<CancellationToken>()),
            Times.Once);
        _carRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<Domain.Models.Car>(c => c.ImageUrl == imageUrl), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenImageUploadFails_ShouldThrowException()
    {
        // Arrange
        var mockFile = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.jpg");

        var command = new CreateCarCommand
        {
            Brand = "BMW",
            Model = "X5",
            Year = 2023,
            FuelType = "Diesel",
            PricePerDay = 120.00m,
            IsAvailable = true,
            Image = mockFile.Object
        };

        var stripeProduct = new Product
        {
            Id = "prod_test123",
            DefaultPriceId = "price_test123"
        };

        _productServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<ProductCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stripeProduct);

        _fileUploadServiceMock
            .Setup(x => x.UploadImageAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Upload failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        _carRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Domain.Models.Car>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSetCorrectStripeIds()
    {
        // Arrange
        var command = new CreateCarCommand
        {
            Brand = "Mercedes",
            Model = "E-Class",
            Year = 2023,
            FuelType = "Petrol",
            PricePerDay = 150.00m,
            IsAvailable = true,
            Image = null
        };

        var stripeProduct = new Product
        {
            Id = "prod_mercedes123",
            DefaultPriceId = "price_mercedes123"
        };

        Domain.Models.Car? capturedCar = null;

        _productServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<ProductCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stripeProduct);

        _carRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Domain.Models.Car>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Models.Car, CancellationToken>((car, _) => capturedCar = car)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedCar.Should().NotBeNull();
        capturedCar!.StripeProductId.Should().Be("prod_mercedes123");
        capturedCar.StripePriceId.Should().Be("price_mercedes123");
    }

    [Fact]
    public async Task Handle_ShouldMapCommandPropertiesToCar()
    {
        // Arrange
        var command = new CreateCarCommand
        {
            Brand = "Audi",
            Model = "A4",
            Year = 2022,
            FuelType = "Diesel",
            PricePerDay = 95.00m,
            IsAvailable = true,
            Image = null
        };

        var stripeProduct = new Product
        {
            Id = "prod_audi123",
            DefaultPriceId = "price_audi123"
        };

        Domain.Models.Car? capturedCar = null;

        _productServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<ProductCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stripeProduct);

        _carRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Domain.Models.Car>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Models.Car, CancellationToken>((car, _) => capturedCar = car)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedCar.Should().NotBeNull();
        capturedCar!.Brand.Should().Be("Audi");
        capturedCar.Model.Should().Be("A4");
        capturedCar.Year.Should().Be(2022);
        capturedCar.FuelType.Should().Be("Diesel");
        capturedCar.PricePerDay.Should().Be(95.00m);
        capturedCar.IsAvailable.Should().BeTrue();
    }
}