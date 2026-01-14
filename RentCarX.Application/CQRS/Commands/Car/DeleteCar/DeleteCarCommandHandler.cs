using MediatR;
using Microsoft.Extensions.Logging;
using RentCarX.Application.CQRS.Commands.Car.DeleteCar;
using RentCarX.Application.Interfaces.Services.File;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.Repositories;

public class DeleteCarCommandHandler : IRequestHandler<DeleteCarCommand, Unit>
{
    private readonly ICarRepository _carRepository;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<DeleteCarCommandHandler> _logger;

    public DeleteCarCommandHandler(
       ICarRepository carRepository,
       IFileUploadService fileUploadService,
       ILogger<DeleteCarCommandHandler> logger)
    {
        _carRepository = carRepository;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteCarCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var entity = await _carRepository.GetCarByIdAsync(request.Id, cancellationToken);

        if (entity == null) throw new NotFoundException("Car", request.Id.ToString());

        if (entity.Reservations != null && entity.Reservations.Any())
        {
            _logger.LogWarning("Admin tried to delete car {CarId} with existing reservations.", request.Id);

            throw new BadRequestException("The car cannot be deleted because it has a reservation and payment history. " +
                "Instead of deleting it, consider marking it as unavailable.");
        }

        if (!string.IsNullOrEmpty(entity.ImageUrl))
        {
            try
            {
                await _fileUploadService.DeleteImageAsync(entity.ImageUrl, cancellationToken);
                _logger.LogInformation("Image deleted for car {CarId}", request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image for car {CarId}", request.Id);
            }
        }

        await _carRepository.RemoveAsync(request.Id, cancellationToken);
        _logger.LogInformation("Car deleted {CarId}", request.Id);

        return Unit.Value;
    }
}
