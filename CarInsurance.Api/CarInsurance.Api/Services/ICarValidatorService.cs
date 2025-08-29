namespace CarInsurance.Api.Services;

public interface ICarValidatorService
{
	Task ValidateCarExistance(long carId);
}
