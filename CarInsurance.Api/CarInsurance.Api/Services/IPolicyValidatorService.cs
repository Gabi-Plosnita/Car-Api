namespace CarInsurance.Api.Services;

public interface IPolicyValidatorService
{
	Task<bool> IsCoveredOnDate(long carId, DateOnly date);
}
