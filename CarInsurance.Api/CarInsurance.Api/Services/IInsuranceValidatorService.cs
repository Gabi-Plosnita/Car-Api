namespace CarInsurance.Api.Services;

public interface IInsuranceValidatorService
{
	Task<bool> IsCoveredOnDateAsync(long carId, DateOnly date);

	Task EnsureIsCoveredOnDateAsync(long carId, DateOnly date);
}
