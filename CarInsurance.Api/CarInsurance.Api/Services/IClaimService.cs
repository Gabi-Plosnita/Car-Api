using CarInsurance.Api.Dtos;

namespace CarInsurance.Api.Services;

public interface IClaimService
{
	Task<InsuranceClaimResponseDto> CreateAsync(long carId, InsuranceClaimRequestDto dto);
}
