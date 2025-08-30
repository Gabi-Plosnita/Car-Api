namespace CarInsurance.Api.Exceptions;

public class DateNotCoveredException : Exception
{
	public DateNotCoveredException() { }

	public DateNotCoveredException(string message)
		: base(message) { }

	public DateNotCoveredException(string message, Exception inner)
		: base(message, inner) { }
}
