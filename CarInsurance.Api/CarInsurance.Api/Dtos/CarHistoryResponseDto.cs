namespace CarInsurance.Api.Dtos;

public record CarHistoryResponseDto
{
	public long CarId { get; init; }
	public string Vin { get; init; } = default!;
	public string? Make { get; init; }
	public string? Model { get; init; }
	public int YearOfManufacture { get; init; }
	public long OwnerId { get; init; }
	public string? OwnerName { get; init; }

	public IReadOnlyList<CarHistoryEventDto> Events { get; init; } = Array.Empty<CarHistoryEventDto>();
}
