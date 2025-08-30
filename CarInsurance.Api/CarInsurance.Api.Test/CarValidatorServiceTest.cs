using AwesomeAssertions;
using CarInsurance.Api.Exceptions;
using CarInsurance.Api.Services;

namespace CarInsurance.Api.Test;

[TestClass]
public class CarValidatorServiceTests : TestBase
{
	[TestMethod]
	public async Task ValidateCarExistanceAsync_ReturnsTrue_WhenCarExists()
	{
		await SeedHelper.AddCarAsync(Db, 1);
		var service = new CarValidatorService(Db);

		var result = await service.ValidateCarExistanceAsync(1);

		result.Should().BeTrue();
	}

	[TestMethod]
	public async Task ValidateCarExistanceAsync_ReturnsFalse_WhenCarDoesNotExists()
	{
		var service = new CarValidatorService(Db);

		var result = await service.ValidateCarExistanceAsync(1);

		result.Should().BeFalse();
	}

	[TestMethod]
	public async Task EnsureCarExistsAsync_DoesNotThrow_WhenCarExists()
	{
		await SeedHelper.AddCarAsync(Db, 1);
		var service = new CarValidatorService(Db);

		Func<Task> act = async () => await service.EnsureCarExistsAsync(1);

		await act.Should().NotThrowAsync();
	}

	[TestMethod]
	public async Task EnsureCarExistsAsync_Throws_WhenCarMissing()
	{
		var service = new CarValidatorService(Db);

		Func<Task> act = async () => await service.EnsureCarExistsAsync(42);

		await act.Should()
			.ThrowAsync<CarNotFoundException>()
			.WithMessage("Car 42 not found");
	}
}
