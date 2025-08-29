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
    public async Task<ActionResult<InsuranceValidityResponse>> IsInsuranceValidAsync([FromRoute] long carId, [FromQuery] string date)
    {
        if (!DateOnly.TryParse(date, out var parsed))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        var valid = await _insuranceService.IsInsuranceValidAsync(carId, parsed);
        return Ok(new InsuranceValidityResponse(carId, parsed.ToString("yyyy-MM-dd"), valid));
    }

	[HttpPost("{carId:long}/claims")]
    public async Task<ActionResult<InsuranceClaimResponseDto>> CreateClaimAsync([FromRoute] long carId, InsuranceClaimRequestDto dto)
        => await _claimService.CreateAsync(carId, dto);
}
