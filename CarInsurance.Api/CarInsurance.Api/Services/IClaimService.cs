using CarInsurance.Api.Dtos;

namespace CarInsurance.Api.Services;

public interface IClaimService
{
	Task<ClaimResponseDto> CreateAsync(long carId, ClaimRequestDto dto);
}
