namespace CarInsurance.Api.Dtos;

public record InsuranceValidityResponse(long CarId, string Date, bool Valid);