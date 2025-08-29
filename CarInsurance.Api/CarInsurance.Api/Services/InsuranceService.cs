using CarInsurance.Api.Data;

namespace CarInsurance.Api.Services;

public class InsuranceService(AppDbContext _db, 
							  ICarValidatorService _carValidator,
							  IInsuranceValidatorService _insuranceValidator) : IInsuranceService
{
	public async Task<bool> IsInsuranceValidAsync(long carId, DateOnly date)
	{
		await _carValidator.ValidateCarExistance(carId);
		return await _insuranceValidator.IsCoveredOnDate(carId, date);
	}
}
