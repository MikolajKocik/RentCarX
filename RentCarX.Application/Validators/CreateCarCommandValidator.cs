using FluentValidation;
using RentCarX.Application.CQRS.Commands.Car.AddCar;

namespace RentCarX.Application.Validators
{
    public class CreateCarCommandValidator : AbstractValidator<CreateCarCommand>
    {
        public CreateCarCommandValidator()
        {
            RuleFor(command => command.CarData).NotNull();
            RuleFor(command => command.CarData.Brand)
                .NotEmpty().WithMessage("Brand is required.");
            RuleFor(command => command.CarData.Model)
                .NotEmpty().WithMessage("Model is required.");
            RuleFor(command => command.CarData.PricePerDay)
                .GreaterThan(0).WithMessage("Price per day must be greater than zero.");
            RuleFor(command => command.CarData.Year)
                .GreaterThan(2000).WithMessage("Year must be a valid year."); 
        }
    }
}
