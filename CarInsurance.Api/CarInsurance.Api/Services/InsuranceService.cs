using CarInsurance.Api.Data;

namespace CarInsurance.Api.Services;

public interface IInsuranceService
{
	Task<bool> IsInsuranceValidAsync(long carId, DateOnly date);
}

public class InsuranceService(AppDbContext _db, 
							  ICarValidatorService _carValidator,
							  IInsuranceValidatorService _insuranceValidator) : IInsuranceService
{
	public async Task<bool> IsInsuranceValidAsync(long carId, DateOnly date)
	{
		await _carValidator.EnsureCarExistsAsync(carId);
		return await _insuranceValidator.IsCoveredOnDateAsync(carId, date);
	}
}
