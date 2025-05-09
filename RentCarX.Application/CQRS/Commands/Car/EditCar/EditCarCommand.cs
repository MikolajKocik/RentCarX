using MediatR;

namespace RentCarX.Application.CQRS.Commands.Car.EditCar
{
    public class EditCarCommand : IRequest 
    {
        public Guid Id { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public string? FuelType { get; set; }
        public decimal? PricePerDay { get; set; }
        public bool? IsAvailable { get; set; }
    }
}
