using FluentValidation;
using RentCarX.Application.CQRS.Commands.Car.AddCar;

namespace RentCarX.Application.Validators
{
    public class CreateCarCommandValidator : AbstractValidator<CreateCarCommand>
    {
        public CreateCarCommandValidator()
        {
            RuleFor(command => command.Brand)
                .NotEmpty().WithMessage("Brand is required.")
                .MaximumLength(100).WithMessage("Brand must not exceed 100 characters.");

            RuleFor(command => command.Model)
                .NotEmpty().WithMessage("Model is required.")
                .MaximumLength(100).WithMessage("Model must not exceed 100 characters.");

            RuleFor(command => command.Year)
                .GreaterThanOrEqualTo(2000).WithMessage("Year must be greater than or equal to 2000.")
                .LessThanOrEqualTo(DateTime.Now.Year + 1).WithMessage("Year must not be in the future.");

            RuleFor(command => command.FuelType)
                .NotEmpty().WithMessage("FuelType is required.")
                .Must(ft => new[] { "Petrol", "Diesel", "Electric", "Hybrid", "LPG" }.Contains(ft))
                .WithMessage("FuelType must be one of: Petrol, Diesel, Electric, Hybrid, LPG.");

            RuleFor(command => command.PricePerDay)
                .GreaterThan(0).WithMessage("PricePerDay must be greater than 0.")
                .LessThanOrEqualTo(10000).WithMessage("PricePerDay must not exceed 10000.");

            RuleFor(command => command.IsAvailable)
                .NotNull().WithMessage("IsAvailable is required.");
        }
    }
}
