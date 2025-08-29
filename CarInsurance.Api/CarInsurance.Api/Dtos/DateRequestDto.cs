using System.ComponentModel.DataAnnotations;

namespace CarInsurance.Api.Dtos;

public class DateRequestDto
{
	[Required]
	[DataType(DataType.Date)]
	public DateOnly Date { get; set; }
}
