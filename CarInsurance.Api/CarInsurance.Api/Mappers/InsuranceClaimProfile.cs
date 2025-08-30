using AutoMapper;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;

namespace CarInsurance.Api.Mappers;

public class InsuranceClaimProfile : Profile
{
	public InsuranceClaimProfile()
	{
		CreateMap<InsuranceClaimRequestDto, InsuranceClaim>();
		CreateMap<InsuranceClaim, InsuranceClaimResponseDto>();
		CreateMap<InsuranceClaim, ClaimEventDto>()
			.ForMember(d => d.ClaimId, o => o.MapFrom(s => s.Id));
	}
}
