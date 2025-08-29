using AutoMapper;
using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;

namespace CarInsurance.Api.Services;

public class ClaimService(AppDbContext _db, IMapper _mapper, ICarValidatorService _carValidator) : IClaimService
{
	public async Task<InsuranceClaimResponseDto> CreateAsync(long carId, InsuranceClaimRequestDto requestDto)
	{
		await _carValidator.ValidateCarExistance(carId);

		var entity = _mapper.Map<InsuranceClaim>(requestDto);
		entity.CarId = carId;

		_db.InsuranceClaims.Add(entity);
		await _db.SaveChangesAsync();

		return _mapper.Map<InsuranceClaimResponseDto>(entity);
	}
}
