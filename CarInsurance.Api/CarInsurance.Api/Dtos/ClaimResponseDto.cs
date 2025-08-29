namespace CarInsurance.Api.Dtos;

public record ClaimResponseDto(long Id, long CarId, DateOnly ClaimDate, string Description, decimal Amount);

