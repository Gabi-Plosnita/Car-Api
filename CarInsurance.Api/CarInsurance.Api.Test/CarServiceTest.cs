using AutoMapper;
using AwesomeAssertions;
using CarInsurance.Api.Mappers;
using CarInsurance.Api.Services;

namespace CarInsurance.Api.Test;

[TestClass]
public class CarServiceTests : TestBase
{
	private IMapper _mapper = default!;
	private CarService _carService = default!;

	private static DateOnly D(int y, int m, int d) => new(y, m, d);

	[TestInitialize]
	public void Init()
	{
		var cfg = new MapperConfiguration(c =>
		{
			c.AddMaps(typeof(CarProfile).Assembly);              
			c.AddMaps(typeof(InsuranceClaimProfile).Assembly);   
			c.AddMaps(typeof(InsuranceClaimProfile).Assembly);        
		});
		_mapper = new Mapper(cfg);
		_carService = new CarService(Db, _mapper);
	}

	[TestMethod]
	public async Task ListCarsAsync_ReturnsProjectedDtos()
	{
		await SeedHelper.AddCarAsync(Db, carId: 1, ownerId: 101, ownerName: "Alice");
		await SeedHelper.AddCarAsync(Db, carId: 2, ownerId: 102, ownerName: "Bob");

		var list = await _carService.ListCarsAsync();

		list.Should().NotBeNull();
		list.Count.Should().Be(2);
		list.Select(c => c.Id).Should().BeEquivalentTo(new[] { 1L, 2L });
	}

	[TestMethod]
	public async Task GetHistoryAsync_ReturnsNull_WhenCarNotFound()
	{
		var dto = await _carService.GetHistoryAsync(999);

		dto.Should().BeNull();
	}

	[TestMethod]
	public async Task GetHistoryAsync_ReturnsHeaderAndEmptyEvents_WhenNoPoliciesOrClaims()
	{
		await SeedHelper.AddCarAsync(Db, carId: 5, ownerId: 200, ownerName: "NoEvents Owner");

		var dto = await _carService.GetHistoryAsync(5);

		dto.Should().NotBeNull();
		dto.CarId.Should().Be(5);
		dto.Events.Should().NotBeNull();
		dto.Events.Should().BeEmpty();
	}

	[TestMethod]
	public async Task GetHistoryAsync_ReturnsChronologicalTimeline_PolicyBeforeClaim_SameDay()
	{
		await SeedHelper.AddCarAsync(Db, carId: 1, ownerId: 300, ownerName: "Owner 1");

		await SeedHelper.AddPolicyAsync(Db, policyId: 10, carId: 1, start: D(2025, 1, 1), end: D(2025, 3, 31)); 
		await SeedHelper.AddPolicyAsync(Db, policyId: 11, carId: 1, start: D(2025, 4, 1), end: D(2025, 12, 31));
		
		await SeedHelper.AddClaimAsync(Db, claimId: 100, carId: 1, claimDate: D(2025, 3, 31), description: "End-day claim", amount: 100m);
		await SeedHelper.AddClaimAsync(Db, claimId: 101, carId: 1, claimDate: D(2025, 4, 1), description: "Start-day claim", amount: 200m);

		var dto = await _carService.GetHistoryAsync(1);

		dto.Should().NotBeNull();
		dto.CarId.Should().Be(1);
		dto.OwnerName.Should().Be("Owner 1");

		dto.Events.Should().HaveCount(4);
		var ev = dto.Events.ToList();

		ev[0].Policy.Should().NotBeNull();
		ev[0].Policy!.PolicyId.Should().Be(10);
		ev[0].Claim.Should().BeNull();

		ev[1].Claim.Should().NotBeNull();
		ev[1].Claim!.ClaimId.Should().Be(100);
		ev[1].Claim!.Description.Should().Be("End-day claim");

		ev[2].Policy.Should().NotBeNull();
		ev[2].Policy!.PolicyId.Should().Be(11);
		ev[2].Claim.Should().BeNull();

		ev[3].Claim.Should().NotBeNull();
		ev[3].Claim!.ClaimId.Should().Be(101);
		ev[3].Claim!.Description.Should().Be("Start-day claim");
	}

	[TestMethod]
	public async Task GetHistoryAsync_IgnoresOtherCarsEvents()
	{
		await SeedHelper.AddCarAsync(Db, carId: 1, ownerId: 400, ownerName: "Target");
		await SeedHelper.AddCarAsync(Db, carId: 2, ownerId: 401, ownerName: "Other");
		await SeedHelper.AddPolicyAsync(Db, policyId: 21, carId: 2, start: D(2025, 1, 1), end: D(2025, 12, 31));
		await SeedHelper.AddClaimAsync(Db, claimId: 201, carId: 2, claimDate: D(2025, 6, 1), description: "other", amount: 50m);

		var dto = await _carService.GetHistoryAsync(1);

		dto.Should().NotBeNull();
		dto.CarId.Should().Be(1);
		dto.Events.Should().BeEmpty(); 
	}
}