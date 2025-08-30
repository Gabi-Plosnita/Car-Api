using CarInsurance.Api.Dtos;

namespace CarInsurance.Api.Services;

public interface ICarService
{
	Task<List<CarResponseDto>> ListCarsAsync();

	Task<CarHistoryResponseDto?> GetHistoryAsync(long carId);
}
