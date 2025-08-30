namespace CarInsurance.Api.Dtos;

public record ClaimEventDto
{
	public long ClaimId { get; init; }
	public DateOnly ClaimDate { get; init; }
	public string Description { get; init; } = string.Empty;
	public decimal Amount { get; init; }
}
