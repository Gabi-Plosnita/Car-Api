using CarInsurance.Api.Services;

namespace CarInsurance.Api.Jobs;

public sealed class PolicyExpirationWorker(ILogger<PolicyExpirationWorker> _logger,
										   IPolicyExpirationProcessor _processor,
										   IClock _clock,
										   TimeZoneInfo _appTimeZone) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			var nowUtc = _clock.UtcNow;
			var nowLocal = TimeZoneInfo.ConvertTime(nowUtc, _appTimeZone);

			var nextLocal = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, 0, 30, 0);
			if (nowLocal >= nextLocal)
				nextLocal = nextLocal.AddDays(1);

			var nextUtc = TimeZoneInfo.ConvertTimeToUtc(nextLocal, _appTimeZone);
			var delay = nextUtc - nowUtc;

			if (delay > TimeSpan.Zero)
			{
				_logger.LogInformation("PolicyExpirationWorker sleeping until {NextUtc:o}", nextUtc);
				try { await Task.Delay(delay, stoppingToken); }
				catch (TaskCanceledException) { break; }
			}

			try
			{
				var count = await _processor.ProcessAsync(stoppingToken);
				_logger.LogInformation("PolicyExpirationWorker processed {Count} expirations", count);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "PolicyExpirationWorker failed");
			}
		}
	}
}
