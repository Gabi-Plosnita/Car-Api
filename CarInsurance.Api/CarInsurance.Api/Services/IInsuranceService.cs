namespace CarInsurance.Api.Services;

public interface IInsuranceService
{
	Task<bool> IsInsuranceValidAsync(long carId, DateOnly date);
}
