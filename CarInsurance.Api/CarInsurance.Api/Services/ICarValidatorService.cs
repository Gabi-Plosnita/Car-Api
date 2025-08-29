namespace CarInsurance.Api.Services;

public interface ICarValidatorService
{
	Task<bool> ValidateCarExistanceAsync(long carId);

	Task EnsureCarExistsAsync(long carId);
}
