namespace CarInsurance.Api.Services;

public interface IInsuranceValidatorService
{
	Task<bool> IsCoveredOnDate(long carId, DateOnly date);

	Task EnsureIsCoveredOnDate(long carId, DateOnly date);
}
