namespace CarInsurance.Api.Dtos;

public record CarResponseDto(long Id, string Vin, string? Make, string? Model, int Year, long OwnerId, string OwnerName, string? OwnerEmail);

