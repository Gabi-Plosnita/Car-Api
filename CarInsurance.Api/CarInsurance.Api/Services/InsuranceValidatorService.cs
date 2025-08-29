using CarInsurance.Api.Data;
using CarInsurance.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class InsuranceValidatorService(AppDbContext _db) : IInsuranceValidatorService
{
	public async Task<bool> IsCoveredOnDate(long carId, DateOnly date)
	{
		return await _db.Policies.AnyAsync(p =>
			p.CarId == carId &&
			p.StartDate <= date &&
			p.EndDate >= date
		);
	}

	public async Task EnsureIsCoveredOnDate(long carId, DateOnly date)
	{
		if(!await IsCoveredOnDate(carId, date))
			throw new DateNotCoveredException($"The date {date:yyyy-MM-dd} is not within the allowed range.");
	}
}

