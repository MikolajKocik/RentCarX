using AutoMapper;
using RentCarX.Application.DTOs.Car;
using RentCarX.Domain.Models;

namespace RentCarX.Application.MappingsProfile
{
    public class CarMappingProfile : Profile
    {
        public CarMappingProfile()
        {
            CreateMap<Car, CarDto>().ReverseMap(); 

            CreateMap<CreateCarDto, Car>().ReverseMap();
            CreateMap<EditCarDto, Car>().ReverseMap();
        }
    }
}
