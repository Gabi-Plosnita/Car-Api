namespace CarInsurance.Api.Dtos;

public record CarHistoryEventDto
{
	public PolicyEventDto? Policy { get; init; }
	public ClaimEventDto? Claim { get; init; }
}