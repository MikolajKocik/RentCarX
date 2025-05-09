using FluentValidation;
using RentCarX.Application.CQRS.Commands.Car.EditCar;

namespace RentCarX.Application.Validators
{
    public class EditCarCommandValidator : AbstractValidator<EditCarCommand>
    {
        public EditCarCommandValidator()
        {
            RuleFor(command => command.Id)
                .NotEmpty().WithMessage("Car ID is required for editing.");
            RuleFor(command => command.CarData).NotNull();
            RuleFor(command => command.CarData.Brand)
                .NotEmpty().WithMessage("Brand is required.");
            RuleFor(command => command.CarData.Model)
                .NotEmpty().WithMessage("Model is required.");
        }
    }
}