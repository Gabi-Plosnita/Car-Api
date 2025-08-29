using AutoMapper;
using AutoMapper.QueryableExtensions;
using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class CarService(AppDbContext _db, IMapper _mapper) : ICarService
{
    public async Task<List<CarResponseDto>> ListCarsAsync()
    {
		return await _db.Cars
			.AsNoTracking()
			.ProjectTo<CarResponseDto>(_mapper.ConfigurationProvider)
			.ToListAsync();
	}
}
