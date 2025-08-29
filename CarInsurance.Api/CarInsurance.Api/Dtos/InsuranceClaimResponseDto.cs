namespace CarInsurance.Api.Dtos;

public record InsuranceClaimResponseDto(long Id, long CarId, DateOnly ClaimDate, string Description, decimal Amount);

