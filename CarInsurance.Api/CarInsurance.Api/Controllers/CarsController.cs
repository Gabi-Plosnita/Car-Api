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
		var claim = await _claimService.CreateAsync(carId, dto);

		return CreatedAtRoute(
			"GetClaimById",
			new { claimId = claim.Id },
			claim
		);
	}

	[HttpGet("claims/{claimId:long}", Name = "GetClaimById")]
	public async Task<ActionResult<InsuranceClaimResponseDto>> GetClaimAsync([FromRoute] long claimId)
	{
		var claim = await _claimService.GetAsync(claimId);
		if (claim == null) return NotFound();
		return Ok(claim);
	}


}
