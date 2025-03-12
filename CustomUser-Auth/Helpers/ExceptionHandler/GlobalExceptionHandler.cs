using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace CustomUser_Auth.Helpers.ExceptionHandler;


public class GlobalExceptionHandler(IHostEnvironment env, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    private const string UnhandledExceptionMsg = "An unhandled exception has occurred while executing the request.";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        AddErrorCode(exception); // Custom method to store error codes

        var problemDetails = CreateProblemDetails(context, exception);
        var json = ToJson(problemDetails);

        const string contentType = "application/problem+json";
        context.Response.ContentType = contentType;
        await context.Response.WriteAsync(json, cancellationToken);

        return true;
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var errorCode = GetErrorCode(exception); // Retrieve the error code
        var statusCode = context.Response.StatusCode;
        var reasonPhrase = ReasonPhrases.GetReasonPhrase(statusCode) ?? UnhandledExceptionMsg;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = reasonPhrase,
            Extensions =
            {
                ["errorCode"] = errorCode
            }
        };

        if (!env.IsDevelopment())
        {
            return problemDetails;
        }

        problemDetails.Detail = exception.ToString();
        problemDetails.Extensions["traceId"] = Activity.Current?.Id;
        problemDetails.Extensions["requestId"] = context.TraceIdentifier;
        problemDetails.Extensions["data"] = exception.Data;

        return problemDetails;
    }

    private string ToJson(ProblemDetails problemDetails)
    {
        try
        {
            return JsonSerializer.Serialize(problemDetails, SerializerOptions);
        }
        catch (Exception ex)
        {
            const string msg = "An exception has occurred while serializing error to JSON";
            logger.LogError(ex, msg);
        }

        return string.Empty;
    }

    // Helper method to add error codes to an exception
    private static void AddErrorCode(Exception exception)
    {
        if (!exception.Data.Contains("ErrorCode"))
        {
            var errorCode = exception switch
            {
                ArgumentNullException => "ERR_NULL_ARGUMENT",
                ArgumentException => "ERR_INVALID_ARGUMENT",
                UnauthorizedAccessException => "ERR_UNAUTHORIZED",
                InvalidOperationException => "ERR_INVALID_OPERATION",
                _ => "ERR_UNKNOWN"
            };

            exception.Data["ErrorCode"] = errorCode;
        }
    }

    // Helper method to retrieve the error code
    private static string GetErrorCode(Exception exception)
    {
        return exception.Data.Contains("ErrorCode")
            ? exception.Data["ErrorCode"]?.ToString() ?? "UnknownError"
            : "UnknownError";
    }
}
