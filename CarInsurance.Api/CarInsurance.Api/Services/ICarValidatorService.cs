namespace CarInsurance.Api.Services;

public interface ICarValidatorService
{
	Task<bool> ValidateCarExistance(long carId);

	Task EnsureCarExists(long carId);
}
