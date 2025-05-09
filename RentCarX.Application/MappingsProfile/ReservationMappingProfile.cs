using AutoMapper;
using RentCarX.Application.DTOs.Reservation;
using RentCarX.Domain.Models;

namespace RentCarX.Application.MappingsProfile
{
    public class ReservationMappingProfile : Profile
    {
        public ReservationMappingProfile()
        {
            CreateMap<Reservation, ReservationDto>()
                .ForMember(dest => dest.CarName, 
                           opt => opt.MapFrom(src => $"{src.Car.Brand} {src.Car.Model}")); 
        }
    }
}
