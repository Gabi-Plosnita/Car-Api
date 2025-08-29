using System.ComponentModel.DataAnnotations;

namespace CarInsurance.Api.Dtos;

public record InsuranceClaimRequestDto
{
	[Required(ErrorMessage = "Claim date is required.")]
	[DataType(DataType.Date, ErrorMessage = "Claim date must be a valid date.")]
	public DateOnly ClaimDate { get; init; }

	[Required(ErrorMessage = "Description is required.")]
	[StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
	public string Description { get; init; } = string.Empty;

	[Range(0.01, 1_000_000, ErrorMessage = "Amount must be between 0.01 and 1,000,000.")]
	public decimal Amount { get; init; }
}
