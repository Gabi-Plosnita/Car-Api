using AwesomeAssertions;
using CarInsurance.Api.Controllers;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CarInsurance.Api.Test.Controllers;

[TestClass]
public class CarsControllerTests
{
	private Mock<ICarService> _carService = default!;
	private Mock<IInsuranceService> _insuranceService = default!;
	private Mock<IClaimService> _claimService = default!;
	private CarsController _controller = default!;

	[TestInitialize]
	public void Init()
	{
		_carService = new Mock<ICarService>(MockBehavior.Strict);
		_insuranceService = new Mock<IInsuranceService>(MockBehavior.Strict);
		_claimService = new Mock<IClaimService>(MockBehavior.Strict);

		_controller = new CarsController(_carService.Object, _insuranceService.Object, _claimService.Object);
	}

	private static DateOnly D(int y, int m, int d) => new(y, m, d);

	[TestMethod]
	public async Task GetCarsAsync_ReturnsOk_WithListFromService()
	{
		var list = new List<CarResponseDto>
		{
			new CarResponseDto(1, "VIN1", null, null, 2020, 100, "Alice", null),
			new CarResponseDto(2, "VIN2", null, null, 2021, 101, "Bob",   null)
		};
		_carService.Setup(s => s.ListCarsAsync()).ReturnsAsync(list);

		var result = await _controller.GetCarsAsync();

		var ok = result.Result as OkObjectResult;
		ok.Should().NotBeNull();
		ok.StatusCode.Should().Be(200);
		ok.Value.Should().BeSameAs(list);
	}

	[TestMethod]
	public async Task IsInsuranceValidAsync_ReturnsOk_WithProjectedResponse()
	{
		long carId = 7;
		var dto = new DateRequestDto { Date = D(2025, 1, 2) };
		_insuranceService.Setup(s => s.IsInsuranceValidAsync(carId, dto.Date)).ReturnsAsync(true);

		var result = await _controller.IsInsuranceValidAsync(carId, dto);

		var ok = result.Result as OkObjectResult;
		ok.Should().NotBeNull();
		ok.StatusCode.Should().Be(200);

		ok.Value.Should().BeOfType<InsuranceValidityResponse>();
		var payload = (InsuranceValidityResponse)ok.Value;
		payload.CarId.Should().Be(carId);
		payload.Date.Should().Be(dto.Date.ToString("yyyy-MM-dd"));
		payload.Valid.Should().BeTrue();
	}

	[TestMethod]
	public async Task CreateClaimAsync_ReturnsCreatedAtRoute_WithResponseDto()
	{
		long carId = 1;
		var req = new InsuranceClaimRequestDto
		{
			ClaimDate = D(2025, 8, 3),
			Description = "desc",
			Amount = 10m
		};

		var createdDto = new InsuranceClaimResponseDto(
			Id: 123,
			CarId: carId,
			ClaimDate: req.ClaimDate,
			Description: req.Description,
			Amount: req.Amount
		);

		_claimService.Setup(s => s.CreateAsync(carId, req)).ReturnsAsync(createdDto);

		var result = await _controller.CreateClaimAsync(carId, req);

		var created = result.Result as CreatedAtRouteResult;
		created.Should().NotBeNull();
		created!.RouteName.Should().Be("GetClaimById");
		created.RouteValues.Should().ContainKey("claimId");
		created.RouteValues!["claimId"].Should().Be(createdDto.Id);
		created.Value.Should().BeSameAs(createdDto);
	}

	[TestMethod]
	public async Task GetClaimAsync_ReturnsOk_WhenFound()
	{
		long claimId = 55;
		var dto = new InsuranceClaimResponseDto(
			Id: claimId, 
			CarId: 1, 
			ClaimDate: D(2025, 1, 1), 
			Description: "ok", 
			Amount: 1m
		);
		_claimService.Setup(s => s.GetAsync(claimId)).ReturnsAsync(dto);

		var result = await _controller.GetClaimAsync(claimId);

		var ok = result.Result as OkObjectResult;
		ok.Should().NotBeNull();
		ok.StatusCode.Should().Be(200);
		ok.Value.Should().BeSameAs(dto);
	}

	[TestMethod]
	public async Task GetClaimAsync_ReturnsNotFound_WhenNull()
	{
		long claimId = 404;
		_claimService.Setup(s => s.GetAsync(claimId)).ReturnsAsync((InsuranceClaimResponseDto?)null);

		var result = await _controller.GetClaimAsync(claimId);

		result.Result.Should().BeOfType<NotFoundResult>();
	}

	[TestMethod]
	public async Task GetHistoryAsync_ReturnsOk_WhenFound()
	{
		long carId = 9;
		var history = new CarHistoryResponseDto
		{
			CarId = carId,
			Vin = "VIN000009",
			Events = new List<CarHistoryEventDto>()
		};
		_carService.Setup(s => s.GetHistoryAsync(carId)).ReturnsAsync(history);

		var result = await _controller.GetHistoryAsync(carId);

		var ok = result.Result as OkObjectResult;
		ok.Should().NotBeNull();
		ok!.Value.Should().BeSameAs(history);
	}

	[TestMethod]
	public async Task GetHistoryAsync_ReturnsNotFound_WhenNull()
	{
		long carId = 404;
		_carService.Setup(s => s.GetHistoryAsync(carId)).ReturnsAsync((CarHistoryResponseDto?)null);

		var result = await _controller.GetHistoryAsync(carId);

		result.Result.Should().BeOfType<NotFoundResult>();
	}
}
