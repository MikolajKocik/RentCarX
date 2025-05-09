using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentCarX.Application.DTOs.Car;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.CQRS.Queries.Car.GetFiltered
{
    public class GetFilteredCarsQueryHandler : IRequestHandler<GetFilteredCarsQuery, List<CarDto>>
    {
        private readonly ICarRepository _carRepository;
        private readonly IMapper _mapper;

        public GetFilteredCarsQueryHandler(ICarRepository carRepository, IMapper mapper)
        {
            _carRepository = carRepository;
            _mapper = mapper;
        }

        public async Task<List<CarDto>> Handle(GetFilteredCarsQuery request, CancellationToken cancellationToken)
        {
            var query = _carRepository.GetFilteredCarsQuery(
                request.Brand,
                request.Model,
                request.FuelType,
                request.MinPrice,
                request.MaxPrice,
                request.IsAvailable);

            
            return await query
                .ProjectTo<CarDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

        }
    }
}
