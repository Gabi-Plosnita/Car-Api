using AwesomeAssertions;
using CarInsurance.Api.Exceptions;
using CarInsurance.Api.Services;
using System.Globalization;

namespace CarInsurance.Api.Test;

[TestClass]
public class InsuranceValidatorServiceTests : TestBase
{
	private static DateOnly D(int y, int m, int d) => new(y, m, d);
	private static DateOnly D(string iso) =>
		DateOnly.ParseExact(iso, "yyyy-M-d", CultureInfo.InvariantCulture);

	[DataTestMethod]
	[DataRow("2025-01-01", true)]   // start boundary
	[DataRow("2025-12-31", true)]   // end boundary
	[DataRow("2025-6-25", true)]   // inside period
	[DataRow("2024-12-31", false)]  // before period
	[DataRow("2026-1-1", false)]  // after period
	public async Task IsCoveredOnDateAsync_ReturnsExpected_WhenSingleFullYearPolicy(string dateIso, bool expected)
	{
		await SeedHelper.AddCarAsync(Db, carId: 1);
		await SeedHelper.AddPolicyAsync(Db, policyId: 10, carId: 1, start: new DateOnly(2025, 1, 1), end: new DateOnly(2025, 12, 31));

		var service = new InsuranceValidatorService(Db);
		var date = D(dateIso);

		var covered = await service.IsCoveredOnDateAsync(1, date);

		covered.Should().Be(expected, $"date {date:yyyy-MM-dd} should map to {expected}");
	}

	[TestMethod]
	public async Task IsCoveredOnDateAsync_ReturnsFalse_WhenNoPolicies()
	{
		await SeedHelper.AddCarAsync(Db, carId: 1);
		var service = new InsuranceValidatorService(Db);

		var covered = await service.IsCoveredOnDateAsync(1, D(2025, 1, 1));

		covered.Should().BeFalse();
	}

	[TestMethod]
	public async Task IsCoveredOnDateAsync_IgnoresOtherCarsPolicies()
	{
		await SeedHelper.AddCarAsync(Db, carId: 1, ownerId: 101);
		await SeedHelper.AddCarAsync(Db, carId: 2, ownerId: 102);
		await SeedHelper.AddPolicyAsync(Db, policyId: 20, carId: 2, start: D(2025, 1, 1), end: D(2025, 12, 31));
		var service = new InsuranceValidatorService(Db);

		var covered = await service.IsCoveredOnDateAsync(1, D(2025, 6, 1));

		covered.Should().BeFalse();
	}

	[TestMethod]
	public async Task IsCoveredOnDateAsync_ReturnsTrue_WhenAnyPolicyCovers()
	{
		await SeedHelper.AddCarAsync(Db, carId: 1);
		await SeedHelper.AddPolicyAsync(Db, policyId: 10, carId: 1, start: D(2025, 1, 1), end: D(2025, 3, 31));
		await SeedHelper.AddPolicyAsync(Db, policyId: 11, carId: 1, start: D(2025, 4, 1), end: D(2025, 12, 31));
		var service = new InsuranceValidatorService(Db);

		var covered = await service.IsCoveredOnDateAsync(1, D(2025, 10, 10));

		covered.Should().BeTrue();
	}

	[TestMethod]
	public async Task EnsureIsCoveredOnDateAsync_DoesNotThrow_WhenCovered()
	{
		await SeedHelper.AddCarAsync(Db, carId: 1);
		await SeedHelper.AddPolicyAsync(Db, policyId: 10, carId: 1, start: D(2025, 1, 1), end: D(2025, 12, 31));
		var service = new InsuranceValidatorService(Db);

		Func<Task> act = async () => await service.EnsureIsCoveredOnDateAsync(1, D(2025, 7, 7));

		await act.Should().NotThrowAsync();
	}

	[TestMethod]
	public async Task EnsureIsCoveredOnDateAsync_Throws_WhenNotCovered()
	{
		await SeedHelper.AddCarAsync(Db, carId: 1);
		var service = new InsuranceValidatorService(Db);

		Func<Task> act = async () => await service.EnsureIsCoveredOnDateAsync(1, D(2025, 7, 7));

		await act.Should()
			.ThrowAsync<DateNotCoveredException>()
			.WithMessage("The date 2025-07-07 is not within the allowed range.");
	}
}
