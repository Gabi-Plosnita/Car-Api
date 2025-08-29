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
	}
}
