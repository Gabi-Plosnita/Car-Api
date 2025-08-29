using CarInsurance.Api.Data;
using CarInsurance.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class CarValidatorService(AppDbContext _db) : ICarValidatorService
{
	public async Task<bool> ValidateCarExistanceAsync(long carId)
		=> await _db.Cars.AnyAsync(c => c.Id == carId);

	public async Task EnsureCarExistsAsync(long carId)
	{
		if (!await ValidateCarExistanceAsync(carId))
			throw new CarNotFoundException($"Car {carId} not found");
	}
}
