using MediatR;
using Microsoft.Extensions.Logging;
using RentCarX.Application.Interfaces.Services.File;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.CQRS.Commands.Car.EditCar;

public class EditCarCommandHandler : IRequestHandler<EditCarCommand, Unit>
{
    private readonly ICarRepository _carRepository;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<EditCarCommandHandler> _logger;

    public EditCarCommandHandler(
        ICarRepository carRepository,
        IFileUploadService fileUploadService,
        ILogger<EditCarCommandHandler> logger)
    {
        _carRepository = carRepository;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    public async Task<Unit> Handle(EditCarCommand request, CancellationToken cancellationToken)
    {
        var car = await _carRepository.GetCarByIdAsync(request.Id, cancellationToken);

        if (car == null)
        {
            throw new NotFoundException("Car", request.Id.ToString());
        }

        if (!string.IsNullOrEmpty(request.Brand))
            car.Brand = request.Brand;

        if (!string.IsNullOrEmpty(request.Model))
            car.Model = request.Model;

        if (request.Year.HasValue)
            car.Year = request.Year.Value;

        if (!string.IsNullOrEmpty(request.FuelType))
            car.FuelType = request.FuelType;

        if (request.PricePerDay.HasValue)
            car.PricePerDay = request.PricePerDay.Value;

        car.IsAvailable = request.IsAvailable;

        if (request.Image != null)
        {
            try
            {
                if (!string.IsNullOrEmpty(car.ImageUrl))
                {
                    await _fileUploadService.DeleteImageAsync(car.ImageUrl, cancellationToken);
                }

                car.ImageUrl = await _fileUploadService.UploadImageAsync(request.Image, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image for car {CarId}", request.Id);
                throw;
            }
        }

        await _carRepository.UpdateCarAsync(car, cancellationToken);

        return Unit.Value;
    }
}