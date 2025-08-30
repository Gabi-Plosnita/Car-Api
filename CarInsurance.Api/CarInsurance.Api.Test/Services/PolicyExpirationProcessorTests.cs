using AwesomeAssertions;
using CarInsurance.Api.Data;
using CarInsurance.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace CarInsurance.Api.Test;

[TestClass]
public sealed class PolicyExpirationProcessorTests : TestBase
{
	private static PolicyExpirationProcessor CreateSut(AppDbContext db, IClock clock, TimeZoneInfo tz)
		=> new(db, Mock.Of<ILogger<PolicyExpirationProcessor>>(), clock, tz);

	[TestMethod]
	public async Task ProcessAsync_ReturnsZero_WhenNoPoliciesAreExpired()
	{
		var utcNow = new DateTime(2025, 09, 01, 06, 00, 00, DateTimeKind.Utc);
		var clock = new Mock<IClock>();
		clock.SetupGet(c => c.UtcNow).Returns(utcNow);

		var sut = CreateSut(Db, clock.Object, TimeZoneInfo.Utc);

		await SeedHelper.AddCarAsync(Db, 1);
		await SeedHelper.AddPolicyAsync(Db, 10, 1, new DateOnly(2025, 08, 01), new DateOnly(2025, 09, 01));

		var count = await sut.ProcessAsync();

		count.Should().Be(0);
		var p10 = await Db.Policies.AsNoTracking().SingleAsync(p => p.Id == 10);
		p10.ExpirationLoggedAtUtc.Should().BeNull();
	}

	[TestMethod]
	public async Task ProcessAsync_ProcessesAllExpiredBeforeToday()
	{
		var utcNow = new DateTime(2025, 09, 01, 06, 00, 00, DateTimeKind.Utc);
		var clock = new Mock<IClock>();
		clock.SetupGet(c => c.UtcNow).Returns(utcNow);

		var sut = CreateSut(Db, clock.Object, TimeZoneInfo.Utc);

		await SeedHelper.AddCarAsync(Db, 2);
		await SeedHelper.AddPolicyAsync(Db, 201, 2, new DateOnly(2025, 08, 01), new DateOnly(2025, 08, 30));
		await SeedHelper.AddPolicyAsync(Db, 202, 2, new DateOnly(2025, 08, 01), new DateOnly(2025, 08, 31));
		await SeedHelper.AddPolicyAsync(Db, 203, 2, new DateOnly(2025, 08, 01), new DateOnly(2025, 09, 01));

		var count = await sut.ProcessAsync();

		count.Should().Be(2);

		var p201 = await Db.Policies.AsNoTracking().SingleAsync(p => p.Id == 201);
		var p202 = await Db.Policies.AsNoTracking().SingleAsync(p => p.Id == 202);
		var p203 = await Db.Policies.AsNoTracking().SingleAsync(p => p.Id == 203);

		p201.ExpirationLoggedAtUtc.Should().Be(utcNow);
		p202.ExpirationLoggedAtUtc.Should().Be(utcNow);
		p203.ExpirationLoggedAtUtc.Should().BeNull();
	}

	[TestMethod]
	public async Task ProcessAsync_SkipsAlreadyLoggedPolicies()
	{
		var utcNow = new DateTime(2025, 09, 01, 06, 00, 00, DateTimeKind.Utc);
		var clock = new Mock<IClock>();
		clock.SetupGet(c => c.UtcNow).Returns(utcNow);

		var sut = CreateSut(Db, clock.Object, TimeZoneInfo.Utc);

		await SeedHelper.AddCarAsync(Db, 3);
		await SeedHelper.AddPolicyAsync(Db, 301, 3, new DateOnly(2025, 08, 01), new DateOnly(2025, 08, 20));

		var expiredPolicy = await Db.Policies.SingleAsync(p => p.Id == 301);
		expiredPolicy.ExpirationLoggedAtUtc = new DateTime(2025, 08, 21, 00, 00, 00, DateTimeKind.Utc);
		await Db.SaveChangesAsync();

		var count = await sut.ProcessAsync();

		count.Should().Be(0);
		var p301 = await Db.Policies.AsNoTracking().SingleAsync(p => p.Id == 301);
		p301.ExpirationLoggedAtUtc.Should().Be(new DateTime(2025, 08, 21, 00, 00, 00, DateTimeKind.Utc));
	}

	[TestMethod]
	public async Task ProcessAsync_IsIdempotent_OnSecondRun()
	{
		var utcNow1 = new DateTime(2025, 09, 01, 06, 00, 00, DateTimeKind.Utc);
		var utcNow2 = utcNow1.AddMinutes(10);
		var current = utcNow1;

		var clock = new Mock<IClock>();
		clock.SetupGet(c => c.UtcNow).Returns(() => current);

		var sut = CreateSut(Db, clock.Object, TimeZoneInfo.Utc);

		await SeedHelper.AddCarAsync(Db, 4);
		await SeedHelper.AddPolicyAsync(Db, 401, 4, new DateOnly(2025, 08, 01), new DateOnly(2025, 08, 31));

		var firstProcessing = await sut.ProcessAsync(); 
		firstProcessing.Should().Be(1);

		var expiredPolicy = await Db.Policies.AsNoTracking().SingleAsync(p => p.Id == 401);
		expiredPolicy.ExpirationLoggedAtUtc.Should().Be(utcNow1);

		current = utcNow2;
		var secondProcessing = await sut.ProcessAsync();
		secondProcessing.Should().Be(0);

		var afterSecond = await Db.Policies.AsNoTracking().SingleAsync(p => p.Id == 401);
		afterSecond.ExpirationLoggedAtUtc.Should().Be(utcNow1);
	}

	[TestMethod]
	public async Task ProcessAsync_UsesApplicationTimeZoneForTodayBoundary()
	{
		var tz = TimeZoneInfo.CreateCustomTimeZone("UTC+05", TimeSpan.FromHours(5), "UTC+05", "UTC+05");
		var utcNow = new DateTime(2025, 01, 02, 00, 10, 00, DateTimeKind.Utc);

		var clock = new Mock<IClock>();
		clock.SetupGet(c => c.UtcNow).Returns(utcNow);

		var sut = CreateSut(Db, clock.Object, tz);

		await SeedHelper.AddCarAsync(Db, 5);
		await SeedHelper.AddPolicyAsync(Db, 501, 5, new DateOnly(2024, 12, 01), new DateOnly(2025, 01, 01));
		await SeedHelper.AddPolicyAsync(Db, 502, 5, new DateOnly(2024, 12, 01), new DateOnly(2025, 01, 02));

		var count = await sut.ProcessAsync();

		count.Should().Be(1);

		var p501 = await Db.Policies.AsNoTracking().SingleAsync(p => p.Id == 501);
		var p502 = await Db.Policies.AsNoTracking().SingleAsync(p => p.Id == 502);

		p501.ExpirationLoggedAtUtc.Should().Be(utcNow);
		p502.ExpirationLoggedAtUtc.Should().BeNull();
	}
}