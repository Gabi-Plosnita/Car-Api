using AutoMapper;
using AwesomeAssertions;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Exceptions;
using CarInsurance.Api.Mappers;
using CarInsurance.Api.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CarInsurance.Api.Test;

[TestClass]
public class ClaimServiceTests : TestBase
{
	private Mock<ICarValidatorService> _carValidator = default!;
	private Mock<IInsuranceValidatorService> _insuranceValidator = default!;
	private IMapper _mapper = default!;
	private ClaimService _claimService = default!;

	private static DateOnly D(int y, int m, int d) => new(y, m, d);

	[TestInitialize]
	public void Init()
	{
		var cfg = new MapperConfiguration(c => c.AddMaps(typeof(InsuranceClaimProfile).Assembly));
		_mapper = new Mapper(cfg);

		_carValidator = new Mock<ICarValidatorService>(MockBehavior.Strict);
		_insuranceValidator = new Mock<IInsuranceValidatorService>(MockBehavior.Strict);
		_claimService = new ClaimService(Db, _mapper, _carValidator.Object, _insuranceValidator.Object);
	}

	[TestMethod]
	public async Task GetAsync_ReturnsMappedDto_WhenClaimExists()
	{
		var claimEntity = SeedHelper.AddClaimAsync(Db, claimId: 1, carId: 1, claimDate: D(2025, 8, 30));

		var claimDto = await _claimService.GetAsync(claimEntity.Id);

		claimDto.Should().NotBeNull();
		claimDto.Id.Should().Be(claimEntity.Id);
		claimDto.CarId.Should().Be(1);
		claimDto.Description.Should().Be("seeded claim");
		claimDto.Amount.Should().Be(123.45m);
	}

	[TestMethod]
	public async Task GetAsync_ReturnsNull_WhenNotFound()
	{
		var dto = await _claimService.GetAsync(999);

		dto.Should().BeNull(); 
	}

	[TestMethod]
	public async Task CreateAsync_PersistsClaim_AndReturnsMappedResponse()
	{
		var carId = 7;
		await SeedHelper.AddCarAsync(Db, carId);
		var req = new InsuranceClaimRequestDto
		{
			ClaimDate = D(2025, 8, 29),
			Description = "created via service",
			Amount = 999m
		};
		_carValidator.Setup(v => v.EnsureCarExistsAsync(carId)).Returns(Task.CompletedTask);
		_insuranceValidator.Setup(v => v.EnsureIsCoveredOnDateAsync(carId, req.ClaimDate)).Returns(Task.CompletedTask);

		var result = await _claimService.CreateAsync(carId, req);

		_carValidator.Verify(v => v.EnsureCarExistsAsync(carId), Times.Once);
		_insuranceValidator.Verify(v => v.EnsureIsCoveredOnDateAsync(carId, req.ClaimDate), Times.Once);

		var saved = await Db.InsuranceClaims.FindAsync(result.Id);
		saved.Should().NotBeNull();
		saved!.CarId.Should().Be(carId);
		saved.ClaimDate.Should().Be(req.ClaimDate);
		saved.Description.Should().Be("created via service");
		saved.Amount.Should().Be(999m);

		result.CarId.Should().Be(carId);
		result.Description.Should().Be("created via service");
		result.Amount.Should().Be(999m);
	}

	[TestMethod]
	public async Task CreateAsync_Propagates_WhenCarDoesNotExist()
	{
		var carId = 42;
		var req = new InsuranceClaimRequestDto
		{
			ClaimDate = D(2025, 7, 7),
			Description = "x",
			Amount = 1m
		};
		_carValidator.Setup(v => v.EnsureCarExistsAsync(carId)).ThrowsAsync(new CarNotFoundException($"Car {carId} not found"));

		Func<Task> act = async () => await _claimService.CreateAsync(carId, req);

		await act.Should().ThrowAsync<CarNotFoundException>().WithMessage("Car 42 not found");
		_insuranceValidator.Verify(v => v.EnsureIsCoveredOnDateAsync(It.IsAny<long>(), It.IsAny<DateOnly>()), Times.Never);
		(await Db.InsuranceClaims.CountAsync()).Should().Be(0);
		_carValidator.Verify(v => v.EnsureCarExistsAsync(carId), Times.Once);
	}

	[TestMethod]
	public async Task CreateAsync_Propagates_WhenDateNotCovered()
	{
		var carId = 5;
		var req = new InsuranceClaimRequestDto
		{
			ClaimDate = D(2025, 1, 1),
			Description = "y",
			Amount = 2m
		};
		_carValidator.Setup(v => v.EnsureCarExistsAsync(carId)).Returns(Task.CompletedTask);
		_insuranceValidator.Setup(v => v.EnsureIsCoveredOnDateAsync(carId, req.ClaimDate)).ThrowsAsync(new DateNotCoveredException("not covered"));
		
		Func<Task> act = async () => await _claimService.CreateAsync(carId, req);

		await act.Should().ThrowAsync<DateNotCoveredException>();
		(await Db.InsuranceClaims.CountAsync()).Should().Be(0);
		_carValidator.Verify(v => v.EnsureCarExistsAsync(carId), Times.Once);
		_insuranceValidator.Verify(v => v.EnsureIsCoveredOnDateAsync(carId, req.ClaimDate), Times.Once);
	}
}
