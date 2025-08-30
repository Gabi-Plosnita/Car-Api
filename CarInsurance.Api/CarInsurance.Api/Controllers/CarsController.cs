using CarInsurance.Api.Dtos;
using CarInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarInsurance.Api.Controllers;

[ApiController]
[Route("api/cars")]
public class CarsController(ICarService _carService, 
                            IInsuranceService _insuranceService,
                            IClaimService _claimService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CarResponseDto>>> GetCarsAsync()
        => Ok(await _carService.ListCarsAsync());

    [HttpGet("{carId:long}/insurance-valid")]
    public async Task<ActionResult<InsuranceValidityResponse>> IsInsuranceValidAsync([FromRoute] long carId, [FromQuery] DateRequestDto dto)
    {
        var isValid = await _insuranceService.IsInsuranceValidAsync(carId, dto.Date);
        return Ok(new InsuranceValidityResponse(carId, dto.Date.ToString("yyyy-MM-dd"), isValid));
    }

	[HttpPost("{carId:long}/claims")]
	public async Task<ActionResult<InsuranceClaimResponseDto>> CreateClaimAsync([FromRoute] long carId, InsuranceClaimRequestDto dto)
	{
		var responseDto = await _claimService.CreateAsync(carId, dto);
		return CreatedAtRoute("GetClaimById", new { claimId = responseDto.Id }, responseDto);
	}

	[HttpGet("claims/{claimId:long}", Name = "GetClaimById")]
	public async Task<ActionResult<InsuranceClaimResponseDto>> GetClaimAsync([FromRoute] long claimId)
	{
		var responseDto = await _claimService.GetAsync(claimId);
		if (responseDto == null) return NotFound();
		return Ok(responseDto);
	}

	[HttpGet("{carId:long}/history")]
	public async Task<ActionResult<CarHistoryResponseDto>> GetHistoryAsync([FromRoute] long carId)
	{
		var history = await _carService.GetHistoryAsync(carId);
		if (history == null) return NotFound();
		return Ok(history);
	}
}
