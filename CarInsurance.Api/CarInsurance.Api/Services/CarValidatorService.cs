using CarInsurance.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class CarValidatorService(AppDbContext _db) : ICarValidatorService
{
	public async Task ValidateCarExistance(long carId)
	{
		var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
		if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");
	}
}
