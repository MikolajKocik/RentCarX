using AutoMapper;
using FluentAssertions;
using RentCarX.Application.DTOs.Reservation;
using RentCarX.Application.MappingsProfile;
using RentCarX.Domain.Models;
using RentCarX.Domain.Models.Enums;
using Xunit;

namespace RentCarX.Tests.UnitTests.Mappings;

public class ReservationMappingProfileTests
{
    private readonly IMapper _mapper;

    public ReservationMappingProfileTests()
    {
        var config = new MapperConfiguration(cfg =>
            cfg.AddProfile<ReservationMappingProfile>());

        _mapper = config.CreateMapper();
    }

    #region ReservationDto Mapping Tests

    [Fact]
    public void MapReservationToReservationDto_ShouldMapAllProperties()
    {
        // Arrange
        var car = new Car
        {
            Id = Guid.NewGuid(),
            Brand = "Honda",
            Model = "Civic",
            ImageUrl = "https://example.com/honda.jpg"
        };

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CarId = car.Id,
            UserId = Guid.NewGuid(),
            Car = car,
            StartDate = new DateTime(2026, 1, 1, 10, 0, 0),
            EndDate = new DateTime(2026, 1, 3, 10, 0, 0),
            TotalCost = 241.00m,
            IsPaid = false,
            Status = ReservationStatus.Pending,
            IsDeleted = false
        };

        // Act
        var dto = _mapper.Map<ReservationDto>(reservation);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(reservation.Id);
        dto.CarId.Should().Be(car.Id);
        dto.CarName.Should().Be("Honda Civic");
        dto.CarImageUrl.Should().Be(car.ImageUrl);
        dto.StartDate.Should().Be(reservation.StartDate);
        dto.EndDate.Should().Be(reservation.EndDate);
        dto.TotalCost.Should().Be(241.00m);
        dto.IsPaid.Should().BeFalse();
        dto.Status.Should().Be("Pending");
        dto.DurationDays.Should().Be(2);
    }

    [Theory]
    [InlineData(ReservationStatus.Pending, "Pending")]
    [InlineData(ReservationStatus.Confirmed, "Confirmed")]
    [InlineData(ReservationStatus.InProgress, "InProgress")]
    [InlineData(ReservationStatus.Completed, "Completed")]
    [InlineData(ReservationStatus.Cancelled, "Cancelled")]
    public void MapReservationStatus_ShouldConvertEnumToString(ReservationStatus status, string expectedString)
    {
        // Arrange
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CarId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Car = new Car { Brand = "Test", Model = "Car" },
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = status
        };

        // Act
        var dto = _mapper.Map<ReservationDto>(reservation);

        // Assert
        dto.Status.Should().Be(expectedString);
    }

    [Fact]
    public void MapReservationWithPaidStatus_ShouldMapCorrectly()
    {
        // Arrange
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CarId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Car = new Car { Brand = "BMW", Model = "X5" },
            StartDate = new DateTime(2025, 12, 26, 10, 0, 0),
            EndDate = new DateTime(2025, 12, 27, 10, 0, 0),
            TotalCost = 160.50m,
            IsPaid = true,
            Status = ReservationStatus.Confirmed
        };

        // Act
        var dto = _mapper.Map<ReservationDto>(reservation);

        // Assert
        dto.IsPaid.Should().BeTrue();
        dto.Status.Should().Be("Confirmed");
    }

    [Fact]
    public void MapReservationWithSameDateStartAndEnd_ShouldHaveMinimumOneDayDuration()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CarId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Car = new Car { Brand = "Test", Model = "Car" },
            StartDate = now,
            EndDate = now,
            TotalCost = 0m,
            Status = ReservationStatus.Pending
        };

        // Act
        var dto = _mapper.Map<ReservationDto>(reservation);

        // Assert
        dto.DurationDays.Should().Be(1);
    }

    [Fact]
    public void MapReservationWith31DaysDuration_ShouldCalculateCorrectly()
    {
        // Arrange
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CarId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Car = new Car { Brand = "Honda", Model = "Civic" },
            StartDate = new DateTime(2026, 1, 1, 10, 0, 0),
            EndDate = new DateTime(2026, 2, 1, 10, 0, 0),
            TotalCost = 3735.50m,
            Status = ReservationStatus.Pending
        };

        // Act
        var dto = _mapper.Map<ReservationDto>(reservation);

        // Assert
        dto.DurationDays.Should().Be(31);
    }

    [Fact]
    public void MapReservationWithNullImageUrl_ShouldHandleGracefully()
    {
        // Arrange
        var car = new Car
        {
            Id = Guid.NewGuid(),
            Brand = "Test",
            Model = "Car",
            ImageUrl = null
        };

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CarId = car.Id,
            UserId = Guid.NewGuid(),
            Car = car,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = ReservationStatus.Pending
        };

        // Act
        var dto = _mapper.Map<ReservationDto>(reservation);

        // Assert
        dto.CarImageUrl.Should().BeNull();
    }

    #endregion

    #region ReservationBriefDto Mapping Tests

    [Fact]
    public void MapReservationToReservationBriefDto_WithConfirmedStatus_ShouldMapCorrectly()
    {
        // Arrange
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CarId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Car = new Car { Brand = "Test", Model = "Car" },
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = ReservationStatus.Confirmed,
            IsPaid = true
        };

        // Act
        var dto = _mapper.Map<ReservationBriefDto>(reservation);

        // Assert
        dto.Status.Should().Be("Confirmed");
        dto.IsPaid.Should().BeTrue();
    }

    [Theory]
    [InlineData(ReservationStatus.Pending, "Pending")]
    [InlineData(ReservationStatus.Confirmed, "Confirmed")]
    [InlineData(ReservationStatus.InProgress, "InProgress")]
    [InlineData(ReservationStatus.Completed, "Completed")]
    [InlineData(ReservationStatus.Cancelled, "Cancelled")]
    public void MapReservationBriefDto_WithAllStatuses_ShouldMapCorrectly(ReservationStatus status, string expectedString)
    {
        // Arrange
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CarId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Car = new Car { Brand = "Test", Model = "Car" },
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = status
        };

        // Act
        var dto = _mapper.Map<ReservationBriefDto>(reservation);

        // Assert
        dto.Status.Should().Be(expectedString);
    }

    #endregion

    #region ReservationDeadlineDto Mapping Tests

    [Fact]
    public void MapReservationToReservationDeadlineDto_ShouldMapCorrectly()
    {
        // Arrange
        var car = new Car
        {
            Id = Guid.NewGuid(),
            Brand = "Volvo",
            Model = "XC60"
        };

        var reservationId = Guid.NewGuid();
        var startDate = new DateTime(2025, 12, 30, 10, 0, 0);

        var reservation = new Reservation
        {
            Id = reservationId,
            CarId = car.Id,
            UserId = Guid.NewGuid(),
            Car = car,
            StartDate = startDate,
            EndDate = new DateTime(2025, 12, 31, 10, 0, 0),
            Status = ReservationStatus.Pending
        };

        // Act
        var dto = _mapper.Map<ReservationDeadlineDto>(reservation);

        // Assert
        dto.ReservationId.Should().Be(reservationId);
        dto.CarName.Should().Be("Volvo XC60");
        dto.Deadline.Should().Be(startDate);
    }

    #endregion

    #region Status & IsPaid Logic Tests

    [Fact]
    public void MapReservation_PendingAndUnpaid_ShouldMapAsIs()
    {
        // Arrange
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CarId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Car = new Car { Brand = "Test", Model = "Car" },
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = ReservationStatus.Pending,
            IsPaid = false
        };

        // Act
        var dto = _mapper.Map<ReservationDto>(reservation);

        // Assert
        dto.Status.Should().Be("Pending");
        dto.IsPaid.Should().BeFalse();
    }

    [Fact]
    public void MapReservation_ConfirmedAndPaid_ShouldMapAsIs()
    {
        // Arrange
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CarId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Car = new Car { Brand = "Test", Model = "Car" },
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = ReservationStatus.Confirmed,
            IsPaid = true
        };

        // Act
        var dto = _mapper.Map<ReservationDto>(reservation);

        // Assert
        dto.Status.Should().Be("Confirmed");
        dto.IsPaid.Should().BeTrue();
    }

    [Fact]
    public void MapReservation_InProgressAndPaid_ShouldMapAsIs()
    {
        // Arrange
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CarId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Car = new Car { Brand = "Test", Model = "Car" },
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = ReservationStatus.InProgress,
            IsPaid = true
        };

        // Act
        var dto = _mapper.Map<ReservationDto>(reservation);

        // Assert
        dto.Status.Should().Be("InProgress");
        dto.IsPaid.Should().BeTrue();
    }

    [Fact]
    public void MapReservation_CompletedAndPaid_ShouldMapAsIs()
    {
        // Arrange
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CarId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Car = new Car { Brand = "Test", Model = "Car" },
            StartDate = DateTime.UtcNow.AddDays(-2),
            EndDate = DateTime.UtcNow.AddDays(-1),
            Status = ReservationStatus.Completed,
            IsPaid = true
        };

        // Act
        var dto = _mapper.Map<ReservationDto>(reservation);

        // Assert
        dto.Status.Should().Be("Completed");
        dto.IsPaid.Should().BeTrue();
    }

    [Fact]
    public void MapReservation_CancelledReservation_ShouldMapCorrectly()
    {
        // Arrange
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CarId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Car = new Car { Brand = "Test", Model = "Car" },
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = ReservationStatus.Cancelled,
            IsPaid = false
        };

        // Act
        var dto = _mapper.Map<ReservationDto>(reservation);

        // Assert
        dto.Status.Should().Be("Cancelled");
        dto.IsPaid.Should().BeFalse();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void MapReservation_WithVeryLongDuration_ShouldCalculateCorrectly()
    {
        // Arrange
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CarId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Car = new Car { Brand = "Test", Model = "Car" },
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            Status = ReservationStatus.Pending
        };

        // Act
        var dto = _mapper.Map<ReservationDto>(reservation);

        // Assert
        dto.DurationDays.Should().Be(364);
    }

    [Fact]
    public void MapReservation_CarNameFormat_ShouldBeBrandAndModel()
    {
        // Arrange
        var cars = new[]
        {
            new Car { Brand = "BMW", Model = "X5" },
            new Car { Brand = "Mazda", Model = "CX3" },
            new Car { Brand = "Opel", Model = "Astra" }
        };

        foreach (var car in cars)
        {
            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                CarId = car.Id,
                UserId = Guid.NewGuid(),
                Car = car,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1),
                Status = ReservationStatus.Pending
            };

            // Act
            var dto = _mapper.Map<ReservationDto>(reservation);

            // Assert
            dto.CarName.Should().Be($"{car.Brand} {car.Model}");
        }
    }

    #endregion
}