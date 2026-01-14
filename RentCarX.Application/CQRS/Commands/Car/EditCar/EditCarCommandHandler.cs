using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using RentCarX.Application.Interfaces.Services.File;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.CQRS.Commands.Car.EditCar;

public class EditCarCommandHandler : IRequestHandler<EditCarCommand, Unit>
{
    private readonly ICarRepository _carRepository;
    private readonly IMapper _mapper;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<EditCarCommandHandler> _logger;

    public EditCarCommandHandler(
        ICarRepository carRepository,
        IMapper mapper,
        IFileUploadService fileUploadService,
        ILogger<EditCarCommandHandler> logger)
    {
        _carRepository = carRepository;
        _mapper = mapper;
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

        _mapper.Map(request.CarData, car);

        if (request.CarData.Image != null)
        {
            try
            {
                if (!string.IsNullOrEmpty(car.ImageUrl))
                {
                    await _fileUploadService.DeleteImageAsync(car.ImageUrl, cancellationToken);
                }

                car.ImageUrl = await _fileUploadService.UploadImageAsync(request.CarData.Image, cancellationToken);
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