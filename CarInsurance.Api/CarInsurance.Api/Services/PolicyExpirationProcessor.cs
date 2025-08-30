using CarInsurance.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public interface IPolicyExpirationProcessor
{
	Task<int> ProcessAsync(CancellationToken ct = default);
}

public class PolicyExpirationProcessor(AppDbContext _db,
									   ILogger<PolicyExpirationProcessor> _logger,
									   IClock _clock,
									   TimeZoneInfo _appTimeZone) : IPolicyExpirationProcessor
{
	public async Task<int> ProcessAsync(CancellationToken ct = default)
	{
		var nowLocal = TimeZoneInfo.ConvertTime(_clock.UtcNow, _appTimeZone);
		var todayLocal = DateOnly.FromDateTime(nowLocal.Date);

		var toProcess = await _db.Policies
			.Where(p => p.EndDate < todayLocal && p.ExpirationLoggedAtUtc == null)
			.Select(p => new { p.Id, p.CarId, p.Provider, p.EndDate })
			.ToListAsync(ct);

		if (toProcess.Count == 0) return 0;

		var utcNow = _clock.UtcNow;
		var ids = toProcess.Select(x => x.Id).ToList();

		var affected = await _db.Policies
			.Where(p => ids.Contains(p.Id) && p.ExpirationLoggedAtUtc == null)
			.ExecuteUpdateAsync(s => s.SetProperty(p => p.ExpirationLoggedAtUtc, utcNow), ct);

		foreach (var p in toProcess)
		{
			_logger.LogInformation(
				"Policy expired (CarId: {CarId}, PolicyId: {PolicyId}, Provider: {Provider}, EndDate: {EndDate:yyyy-MM-dd})",
				p.CarId, p.Id, p.Provider, p.EndDate);
		}

		return affected;
	}
}