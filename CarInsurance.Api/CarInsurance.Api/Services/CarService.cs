using AutoMapper;
using AutoMapper.QueryableExtensions;
using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public interface ICarService
{
	Task<List<CarResponseDto>> ListCarsAsync();
	Task<CarHistoryResponseDto?> GetHistoryAsync(long carId);
}

public class CarService(AppDbContext _db, IMapper _mapper) : ICarService
{
    public async Task<List<CarResponseDto>> ListCarsAsync()
    {
		return await _db.Cars
			.AsNoTracking()
			.ProjectTo<CarResponseDto>(_mapper.ConfigurationProvider)
			.ToListAsync();
	}

	public async Task<CarHistoryResponseDto?> GetHistoryAsync(long carId)
	{
		var car = await _db.Cars
			.AsNoTracking()
			.Include(c => c.Owner)
			.Include(c => c.Policies)
			.Include(c => c.InsuranceClaims)
			.FirstOrDefaultAsync(c => c.Id == carId);

		if (car == null) return null;

		var carHistoryDto = _mapper.Map<CarHistoryResponseDto>(car);

		carHistoryDto = carHistoryDto with
		{
			Events = car.Policies
				.Select(p => _mapper.Map<CarHistoryEventDto>(p))
				.Concat(car.InsuranceClaims.Select(cl => _mapper.Map<CarHistoryEventDto>(cl)))
				.OrderBy(e => e.Policy?.StartDate ?? e.Claim!.ClaimDate)
				.ThenBy(e => e.Claim != null) 
				.ToList()
		};

		return carHistoryDto;
	}
}
