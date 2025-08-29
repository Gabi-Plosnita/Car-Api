using AutoMapper;
using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Exceptions;
using CarInsurance.Api.Models;

namespace CarInsurance.Api.Services;

public class ClaimService(AppDbContext _db, 
						  IMapper _mapper, 
						  ICarValidatorService _carValidator,
						  IInsuranceValidatorService _insuranceValidator) : IClaimService
{
	public async Task<InsuranceClaimResponseDto> CreateAsync(long carId, InsuranceClaimRequestDto dto)
	{
		await _carValidator.EnsureCarExists(carId);
		await _insuranceValidator.EnsureIsCoveredOnDate(carId, dto.ClaimDate);

		var entity = _mapper.Map<InsuranceClaim>(dto);
		entity.CarId = carId;

		_db.InsuranceClaims.Add(entity);
		await _db.SaveChangesAsync();

		return _mapper.Map<InsuranceClaimResponseDto>(entity);
	}
}
