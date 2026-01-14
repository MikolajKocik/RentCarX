using AutoMapper;
using RentCarX.Application.DTOs.Reservation;
using RentCarX.Domain.Models;

namespace RentCarX.Application.MappingsProfile;

public class ReservationMappingProfile : Profile
{
    public ReservationMappingProfile()
    {
        CreateMap<Reservation, ReservationDto>()
            .ForMember(dest => dest.CarName,
                       opt => opt.MapFrom(src => $"{src.Car.Brand} {src.Car.Model}"))
            .ForMember(dest => dest.CarImageUrl,
                opt => opt.MapFrom(src => src.Car.ImageUrl))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.DurationDays,
                opt => opt.MapFrom(src => Math.Max(1, (int)(src.EndDate.Date - src.StartDate.Date).Days)));

        CreateMap<Reservation, ReservationBriefDto>()
           .ForMember(dest => dest.Status,
               opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<Reservation, ReservationDeadlineDto>()
           .ForMember(dest => dest.CarName,
               opt => opt.MapFrom(src => $"{src.Car.Brand} {src.Car.Model}"))
           .ForMember(dest => dest.Deadline,
               opt => opt.MapFrom(src => src.StartDate))
           .ForMember(dest => dest.ReservationId,
               opt => opt.MapFrom(src => src.Id));
    }
}
