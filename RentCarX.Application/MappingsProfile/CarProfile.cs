using AutoMapper;
using RentCarX.Application.DTOs.Car;
using RentCarX.Domain.Models;

namespace RentCarX.Application.MappingsProfile
{
    public class CarProfile : Profile
    {
        public CarProfile()
        {
            CreateMap<Car, CarDto>().ReverseMap(); 

            CreateMap<CreateCarDto, Car>().ReverseMap();
            CreateMap<EditCarDto, Car>().ReverseMap();
        }
    }
}
