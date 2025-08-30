using CarInsurance.Api.Jobs;
using CarInsurance.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace CarInsurance.Api.Test;

[TestClass]
public sealed class PolicyExpirationWorkerTests
{
	private sealed class TestablePolicyExpirationWorker : PolicyExpirationWorker
	{
		public TestablePolicyExpirationWorker(
			ILogger<PolicyExpirationWorker> logger,
			IServiceScopeFactory scopeFactory,
			IClock clock,
			TimeZoneInfo tz,
			TimeProvider timeProvider)
			: base(logger, scopeFactory, clock, tz, timeProvider) { }

		public Task RunAsync(CancellationToken token) => base.StartAsync(token);
	}

	private static (TestablePolicyExpirationWorker worker,
					Mock<IServiceScopeFactory> scopeFactoryMock,
					Mock<IServiceScope> scopeMock,
					Mock<IPolicyExpirationProcessor> processorMock,
					Mock<IServiceProvider> providerMock,
					Mock<IClock> clockMock,
					FakeTimeProvider fakeTime,
					CancellationTokenSource cts)
		CreateSut(DateTimeOffset startUtc, TimeZoneInfo tz, Action? cancelAfterProcess = null)
	{
		var fakeTime = new FakeTimeProvider(startUtc);
		var clockMock = new Mock<IClock>();
		clockMock.SetupGet(c => c.UtcNow).Returns(() => fakeTime.GetUtcNow().UtcDateTime);

		var processorMock = new Mock<IPolicyExpirationProcessor>();
		processorMock
			.Setup(p => p.ProcessAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(1)
			.Callback(() => cancelAfterProcess?.Invoke());

		var providerMock = new Mock<IServiceProvider>();
		providerMock
			.Setup(p => p.GetService(typeof(IPolicyExpirationProcessor)))
			.Returns(processorMock.Object);

		var scopeMock = new Mock<IServiceScope>();
		scopeMock.SetupGet(s => s.ServiceProvider).Returns(providerMock.Object);

		var scopeFactoryMock = new Mock<IServiceScopeFactory>();
		scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

		var worker = new TestablePolicyExpirationWorker(
			Mock.Of<ILogger<PolicyExpirationWorker>>(),
			scopeFactoryMock.Object,
			clockMock.Object,
			tz,
			fakeTime);

		var cts = new CancellationTokenSource();
		return (worker, scopeFactoryMock, scopeMock, processorMock, providerMock, clockMock, fakeTime, cts);
	}

	[TestMethod]
	public async Task RunsImmediately_WhenNowIsExactly_00_30_Local()
	{
		var tz = TimeZoneInfo.Utc;
		var scheduledUtc = new DateTime(2025, 9, 1, 0, 30, 0, DateTimeKind.Utc);

		CancellationTokenSource? ctsRef = null;
		var setup = CreateSut(scheduledUtc, tz, cancelAfterProcess: () => ctsRef!.Cancel());
		ctsRef = setup.cts;

		await setup.worker.RunAsync(setup.cts.Token);

		setup.processorMock.Verify(p => p.ProcessAsync(It.IsAny<CancellationToken>()), Times.Once);
		setup.scopeFactoryMock.Verify(f => f.CreateScope(), Times.Once);
	}

	[TestMethod]
	public async Task SleepsUntil_00_30_Local_ThenRuns_WithoutRealWait()
	{
		var tz = TimeZoneInfo.Utc;
		var startUtc = new DateTimeOffset(2025, 9, 1, 0, 10, 0, TimeSpan.Zero); // 20 min before run

		CancellationTokenSource? ctsRef = null;
		var setup = CreateSut(startUtc, tz, cancelAfterProcess: () => ctsRef!.Cancel());
		ctsRef = setup.cts;

		var runTask = setup.worker.RunAsync(setup.cts.Token);

		setup.fakeTime.Advance(TimeSpan.FromMinutes(20) + TimeSpan.FromSeconds(1));

		await runTask;

		setup.processorMock.Verify(p => p.ProcessAsync(It.IsAny<CancellationToken>()), Times.Once);
		setup.scopeFactoryMock.Verify(f => f.CreateScope(), Times.Once);
	}

	[TestMethod]
	public async Task IfPast_00_30_Local_SleepsUntilTomorrow_UnlessCancelled()
	{
		var tz = TimeZoneInfo.Utc;
		var startUtc = new DateTimeOffset(2025, 9, 1, 1, 10, 0, TimeSpan.Zero); // after 00:30

		var setup = CreateSut(startUtc, tz);
		var runTask = setup.worker.RunAsync(setup.cts.Token);

		setup.fakeTime.Advance(TimeSpan.FromHours(1));
		setup.cts.Cancel();

		await runTask;

		setup.processorMock.Verify(p => p.ProcessAsync(It.IsAny<CancellationToken>()), Times.Never);
		setup.scopeFactoryMock.Verify(f => f.CreateScope(), Times.Never);
	}

	[TestMethod]
	public async Task HonorsTimeZone_LocalMidnight_OffsetFromUtc()
	{
		var tz = TimeZoneInfo.CreateCustomTimeZone("UTC+05", TimeSpan.FromHours(5), "UTC+05", "UTC+05");

		var localScheduled = new DateTimeOffset(2025, 1, 2, 0, 30, 0, tz.BaseUtcOffset);
		var utcAtStart = localScheduled.ToUniversalTime().AddMinutes(-15); // 15 min before local run

		CancellationTokenSource? ctsRef = null;
		var setup = CreateSut(utcAtStart, tz, cancelAfterProcess: () => ctsRef!.Cancel());
		ctsRef = setup.cts;

		var runTask = setup.worker.RunAsync(setup.cts.Token);

		setup.fakeTime.Advance(TimeSpan.FromMinutes(16)); // cross the boundary

		await runTask;

		setup.processorMock.Verify(p => p.ProcessAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}