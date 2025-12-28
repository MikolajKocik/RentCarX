using AutoMapper;
using RentCarX.Application.DTOs.Car;
using RentCarX.Domain.Models;

namespace RentCarX.Application.MappingsProfile
{
    public class CarMappingProfile : Profile
    {
        public CarMappingProfile()
        {
            CreateMap<Car, CarDto>()
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailableFlag == 1));

            CreateMap<CreateCarDto, Car>()
                .ForMember(dest => dest.IsAvailableFlag, opt => opt.MapFrom(src => src.IsAvailable ? 1 : 0));

            CreateMap<EditCarDto, Car>()
                .ForMember(dest => dest.IsAvailableFlag, opt => opt.MapFrom(src => src.IsAvailable ? 1 : 0))
                .ReverseMap()
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailableFlag == 1));
        }
    }
}
