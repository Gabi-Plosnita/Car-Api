namespace CarInsurance.Api.Dtos;

public record PolicyEventDto
{
	public long PolicyId { get; init; }
	public string? Provider { get; init; }
	public DateOnly StartDate { get; init; }
	public DateOnly EndDate { get; init; }
}
