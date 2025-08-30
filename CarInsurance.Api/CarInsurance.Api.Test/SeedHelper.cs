using CarInsurance.Api.Data;
using CarInsurance.Api.Models;

namespace CarInsurance.Api.Test;

public static class SeedHelper
{
	public static async Task AddCarAsync(
		AppDbContext db, 
		long carId, 
		long ownerId = 100, 
		string ownerName = "Owner")
	{
		db.Cars.Add(new Car
		{
			Id = carId,
			Vin = $"VIN{carId:D6}",
			YearOfManufacture = 2020,
			Owner = new Owner { Id = ownerId, Name = ownerName }
		});
		await db.SaveChangesAsync();
	}

	public static async Task AddPolicyAsync(
		AppDbContext db, 
		long policyId, 
		long carId, 
		DateOnly start, 
		DateOnly end, 
		string? provider = null)
	{
		db.Policies.Add(new InsurancePolicy
		{
			Id = policyId,
			CarId = carId,
			StartDate = start,
			EndDate = end,
			Provider = provider
		});
		await db.SaveChangesAsync();
	}

	public static async Task<InsuranceClaim> AddClaimAsync(
		AppDbContext db,
		long claimId,
		long carId,
		DateOnly? claimDate = null,
		string description = "seeded claim",
		decimal amount = 123.45m)
	{
		var entity = new InsuranceClaim
		{
			Id = claimId,
			CarId = carId,
			ClaimDate = claimDate ?? new DateOnly(2025, 8, 30),
			Description = description,
			Amount = amount
		};

		db.InsuranceClaims.Add(entity);
		await db.SaveChangesAsync(); 
		return entity;
	}
}
