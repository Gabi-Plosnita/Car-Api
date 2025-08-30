using AwesomeAssertions;
using CarInsurance.Api.Exceptions;
using CarInsurance.Api.Services;
using Moq;

namespace CarInsurance.Api.Test;

[TestClass]
public class InsuranceServiceTests
{
	private Mock<ICarValidatorService> _carValidator = default!;
	private Mock<IInsuranceValidatorService> _insuranceValidator = default!;
	private InsuranceService _insuranceService = default!;

	[TestInitialize]
	public void Init()
	{
		_carValidator = new Mock<ICarValidatorService>(MockBehavior.Strict);
		_insuranceValidator = new Mock<IInsuranceValidatorService>(MockBehavior.Strict);
		_insuranceService = new InsuranceService(_carValidator.Object, _insuranceValidator.Object);
	}
	private static DateOnly D(int y, int m, int d) => new(y, m, d);

	[TestMethod]
	public async Task IsInsuranceValidAsync_ReturnsTrue_WhenCarExists_AndDateIsCovered()
	{
		var carId = 1L;
		var date = D(2025, 6, 15);
		_carValidator.Setup(v => v.EnsureCarExistsAsync(carId)).Returns(Task.CompletedTask);
		_insuranceValidator.Setup(v => v.IsCoveredOnDateAsync(carId, date)).ReturnsAsync(true);

		var result = await _insuranceService.IsInsuranceValidAsync(carId, date);

		result.Should().BeTrue();
		_carValidator.Verify(v => v.EnsureCarExistsAsync(carId), Times.Once);
		_insuranceValidator.Verify(v => v.IsCoveredOnDateAsync(carId, date), Times.Once);
	}

	[TestMethod]
	public async Task IsInsuranceValidAsync_ReturnsFalse_WhenCarExists_ButDateNotCovered()
	{
		var carId = 2L;
		var date = D(2025, 1, 1);
		_carValidator.Setup(v => v.EnsureCarExistsAsync(carId)).Returns(Task.CompletedTask);
		_insuranceValidator.Setup(v => v.IsCoveredOnDateAsync(carId, date)).ReturnsAsync(false);

		var result = await _insuranceService.IsInsuranceValidAsync(carId, date);

		result.Should().BeFalse();
		_carValidator.Verify(v => v.EnsureCarExistsAsync(carId), Times.Once);
		_insuranceValidator.Verify(v => v.IsCoveredOnDateAsync(carId, date), Times.Once);
	}

	[TestMethod]
	public async Task IsInsuranceValidAsync_Propagates_WhenCarDoesNotExist_AndDoesNotCallCoverage()
	{
		var carId = 42L;
		var date = D(2025, 7, 7);
		_carValidator.Setup(v => v.EnsureCarExistsAsync(carId)).ThrowsAsync(new CarNotFoundException($"Car {carId} not found"));
		
		Func<Task> act = async () => await _insuranceService.IsInsuranceValidAsync(carId, date);

		await act.Should().ThrowAsync<CarNotFoundException>().WithMessage("Car 42 not found");

		_insuranceValidator.Verify(v => v.IsCoveredOnDateAsync(It.IsAny<long>(), It.IsAny<DateOnly>()), Times.Never);
		_carValidator.Verify(v => v.EnsureCarExistsAsync(carId), Times.Once);
	}
}
