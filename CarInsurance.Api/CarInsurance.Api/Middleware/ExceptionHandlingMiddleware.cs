using CarInsurance.Api.Exceptions;
using System.Net;
using System.Text.Json;

namespace CarInsurance.Api.Middleware;

public class ExceptionHandlingMiddleware
{
	private readonly RequestDelegate _next;

	public ExceptionHandlingMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context); 
		}
		catch (Exception ex)
		{
			await HandleExceptionAsync(context, ex);
		}
	}

	private static Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		HttpStatusCode statusCode;
		string message = exception.Message;

		switch (exception)
		{
			case CarNotFoundException:
				statusCode = HttpStatusCode.NotFound; 
				break;
			case DateNotCoveredException:
				statusCode = HttpStatusCode.BadRequest; 
				break;
			default:
				statusCode = HttpStatusCode.InternalServerError; 
				break;
		}

		var result = JsonSerializer.Serialize(new
		{
			error = message,
			exception = exception.GetType().Name
		});

		context.Response.ContentType = "application/json";
		context.Response.StatusCode = (int)statusCode;

		return context.Response.WriteAsync(result);
	}
}
