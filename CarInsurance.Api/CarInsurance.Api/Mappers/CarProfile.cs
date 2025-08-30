using AutoMapper;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;

namespace CarInsurance.Api.Mappers;

public class CarProfile : Profile
{
	public CarProfile()
	{
		CreateMap<Car, CarResponseDto>()
			.ForCtorParam("OwnerId", opt => opt.MapFrom(src => src.Owner.Id))
			.ForCtorParam("OwnerName", opt => opt.MapFrom(src => src.Owner.Name))
			.ForCtorParam("OwnerEmail", opt => opt.MapFrom(src => src.Owner.Email));

		CreateMap<Car, CarHistoryResponseDto>()
			.ForMember(d => d.CarId, o => o.MapFrom(s => s.Id))
			.ForMember(d => d.OwnerName, o => o.MapFrom(s => s.Owner.Name))
			.ForMember(d => d.Events, o => o.Ignore());

		CreateMap<InsurancePolicy, CarHistoryEventDto>()
			.ForMember(d => d.Policy, opt => opt.MapFrom(s => s))
			.ForMember(d => d.Claim, opt => opt.Ignore());

		CreateMap<InsuranceClaim, CarHistoryEventDto>()
			.ForMember(d => d.Claim, opt => opt.MapFrom(s => s))
			.ForMember(d => d.Policy, opt => opt.Ignore());
	}
}
