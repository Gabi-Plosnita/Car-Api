using CarInsurance.Api.Data;
using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class InsuranceService(AppDbContext _db, ICarValidatorService _carValidatorService) : IInsuranceService
{
	public async Task<bool> IsInsuranceValidAsync(long carId, DateOnly date)
	{
		await _carValidatorService.ValidateCarExistance(carId);
		return await _db.Policies.AnyAsync(p => p.CarId == carId && ValidatePolicyDate(p, date));
	}

	private bool ValidatePolicyDate(InsurancePolicy policy, DateOnly date)
		=> policy.StartDate <= date && policy.EndDate >= date;
}
