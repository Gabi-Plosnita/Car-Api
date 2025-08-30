using AwesomeAssertions;
using CarInsurance.Api.Exceptions;
using CarInsurance.Api.Services;

namespace CarInsurance.Api.Test;

[TestClass]
public class CarValidatorServiceTests : TestBase
{
	private CarValidatorService _service = default!;

	[TestInitialize]
	public void Init()
	{
		_service = new CarValidatorService(Db);
	}

	[TestMethod]
	public async Task ValidateCarExistanceAsync_ReturnsTrue_WhenCarExists()
	{
		await SeedHelper.AddCarAsync(Db, 1);

		var result = await _service.ValidateCarExistanceAsync(1);

		result.Should().BeTrue();
	}

	[TestMethod]
	public async Task ValidateCarExistanceAsync_ReturnsFalse_WhenCarDoesNotExists()
	{
		var result = await _service.ValidateCarExistanceAsync(1);

		result.Should().BeFalse();
	}

	[TestMethod]
	public async Task EnsureCarExistsAsync_DoesNotThrow_WhenCarExists()
	{
		await SeedHelper.AddCarAsync(Db, 1);

		Func<Task> act = async () => await _service.EnsureCarExistsAsync(1);

		await act.Should().NotThrowAsync();
	}

	[TestMethod]
	public async Task EnsureCarExistsAsync_Throws_WhenCarMissing()
	{
		Func<Task> act = async () => await _service.EnsureCarExistsAsync(42);

		await act.Should()
			.ThrowAsync<CarNotFoundException>()
			.WithMessage("Car 42 not found");
	}
}
