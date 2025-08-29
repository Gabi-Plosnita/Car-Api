using CarInsurance.Api.Data;
using CarInsurance.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class CarValidatorService(AppDbContext _db) : ICarValidatorService
{
	public async Task<bool> ValidateCarExistance(long carId)
		=> await _db.Cars.AnyAsync(c => c.Id == carId);

	public async Task EnsureCarExists(long carId)
	{
		if (!await ValidateCarExistance(carId))
			throw new CarNotFoundException($"Car {carId} not found");
	}
}
