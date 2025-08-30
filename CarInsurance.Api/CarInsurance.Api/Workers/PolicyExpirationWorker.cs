using CarInsurance.Api.Services;

namespace CarInsurance.Api.Jobs;

public class PolicyExpirationWorker(ILogger<PolicyExpirationWorker> _logger,
									IServiceScopeFactory _scopeFactory,
									IClock _clock,
									TimeZoneInfo _appTimeZone,
									TimeProvider _timeProvider) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			var nowUtc = _clock.UtcNow;
			var nowLocal = TimeZoneInfo.ConvertTime(nowUtc, _appTimeZone);

			/*This task run each day at 00:30. If you want to test the loging functionality on the database
			  you can simply adjust the hour and minute and the logic will be called and expired policies will
			  be logged.
			 */
			var nextLocal = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, 0, 30, 0);
			if (nowLocal >= nextLocal) nextLocal = nextLocal.AddDays(1);

			var nextUtc = TimeZoneInfo.ConvertTimeToUtc(nextLocal, _appTimeZone);
			var delay = nextUtc - nowUtc;

			if (delay > TimeSpan.Zero)
			{
				_logger.LogInformation("PolicyExpirationWorker sleeping until {NextUtc:o}", nextUtc);
				try { await Task.Delay(delay, _timeProvider, stoppingToken); } catch (TaskCanceledException) { break; }
			}

			try
			{
				using var scope = _scopeFactory.CreateScope();
				var processor = scope.ServiceProvider.GetRequiredService<IPolicyExpirationProcessor>();
				var count = await processor.ProcessAsync(stoppingToken);
				_logger.LogInformation("PolicyExpirationWorker processed {Count} expirations", count);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "PolicyExpirationWorker failed");
			}
		}
	}
}
