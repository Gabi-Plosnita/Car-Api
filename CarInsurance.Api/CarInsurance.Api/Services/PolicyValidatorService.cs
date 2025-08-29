using CarInsurance.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class PolicyValidatorService(AppDbContext _db) : IPolicyValidatorService
{
	public async Task<bool> IsCoveredOnDate(long carId, DateOnly date)
	{
		return await _db.Policies.AnyAsync(p =>
			p.CarId == carId &&
			p.StartDate <= date &&
			p.EndDate >= date
		);
	}
}

