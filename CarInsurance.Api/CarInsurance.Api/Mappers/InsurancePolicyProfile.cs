using AutoMapper;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;

namespace CarInsurance.Api.Mappers;

public class InsurancePolicyProfile : Profile
{
	public InsurancePolicyProfile()
	{
		CreateMap<InsurancePolicy, PolicyEventDto>()
			.ForMember(d => d.PolicyId, o => o.MapFrom(s => s.Id));
	}
}
