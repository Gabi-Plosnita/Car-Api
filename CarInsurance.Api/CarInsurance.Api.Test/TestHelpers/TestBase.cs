using CarInsurance.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Test;

public abstract class TestBase
{
	private SqliteConnection _conn = default!;
	protected AppDbContext Db { get; private set; } = default!;

	[TestInitialize]
	public void Initialize()
	{
		_conn = new SqliteConnection("Data Source=:memory:");
		_conn.Open();

		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseSqlite(_conn)
			.Options;

		Db = new AppDbContext(options);

		Db.Database.EnsureCreated();
	}

	[TestCleanup]
	public void Cleanup()
	{
		Db?.Dispose();
		_conn?.Dispose(); 
	}
}
