using CarInsurance.Api.Dtos;

namespace CarInsurance.Api.Services;

public interface ICarService
{
	Task<List<CarDto>> ListCarsAsync();
}
