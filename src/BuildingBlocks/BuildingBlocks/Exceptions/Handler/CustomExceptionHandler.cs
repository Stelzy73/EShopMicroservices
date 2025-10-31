using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Exceptions.Handler;

public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError("Error Message: {exceptionMessage}, Time of occurence: {time}", exception.Message, DateTime.UtcNow);

        var statusCode = GetStatusCode(exception, context);
        var problemDetails = GetProblemDetails(exception, statusCode, context);

        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);
        return true;
    }

    private static ProblemDetails GetProblemDetails(Exception exception, int? statusCode, HttpContext context)
    {
        var problemDetails = new ProblemDetails
        {
            Title = exception.Message,
            Detail = exception.GetType().Name,
            Status = statusCode,
            Instance = context.Request.Path
        };
        problemDetails.Extensions.Add("traceId", context.TraceIdentifier);

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions.Add("ValidationErrors", validationException.Errors);
        }

        return problemDetails;
    }

    private static int? GetStatusCode(Exception exception, HttpContext context)
    {
        return exception switch
        {
            InternalServerException =>
            (
                context.Response.StatusCode = StatusCodes.Status500InternalServerError
            ),
            ValidationException =>
            (
                context.Response.StatusCode = StatusCodes.Status400BadRequest
            ),
            BadRequestException =>
            (
                context.Response.StatusCode = StatusCodes.Status400BadRequest
            ),
            NotFoundException =>
            (
                context.Response.StatusCode = StatusCodes.Status404NotFound
            ),
            _ =>
            (
                context.Response.StatusCode = StatusCodes.Status500InternalServerError
            )
        };
    }
}