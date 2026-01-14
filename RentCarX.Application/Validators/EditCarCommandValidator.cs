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

            RuleFor(command => command.Brand)
                .MaximumLength(100).WithMessage("Brand must not exceed 100 characters.")
                .When(command => !string.IsNullOrEmpty(command.Brand));

            RuleFor(command => command.Model)
                .MaximumLength(100).WithMessage("Model must not exceed 100 characters.")
                .When(command => !string.IsNullOrEmpty(command.Model));

            RuleFor(command => command.Year)
                .GreaterThanOrEqualTo(2000).WithMessage("Year must be greater than or equal to 2000.")
                .LessThanOrEqualTo(DateTime.Now.Year + 1).WithMessage("Year must not be in the future.")
                .When(command => command.Year.HasValue);

            RuleFor(command => command.FuelType)
                .Must(ft => new[] { "Petrol", "Diesel", "Electric", "Hybrid", "LPG" }.Contains(ft))
                .WithMessage("FuelType must be one of: Petrol, Diesel, Electric, Hybrid, LPG.")
                .When(command => !string.IsNullOrEmpty(command.FuelType));

            RuleFor(command => command.PricePerDay)
                 .GreaterThan(0).WithMessage("PricePerDay must be greater than 0.")
                 .LessThanOrEqualTo(10000).WithMessage("PricePerDay must not exceed 10000.")
                 .When(command => command.PricePerDay.HasValue);

            RuleFor(command => command.IsAvailable)
                .NotNull().WithMessage("IsAvailable is required.");
        }
    }
}