using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;

namespace UserRegistration.Api.Logging;

public class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        logger.LogError(LogEvents.RequestFailed,
            "Не удалось обработать запрос. Тип ошибки: {ExceptionType}. TraceId: {TraceId}.",
            exception.GetType().Name, traceId);

        await Results.Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Не удалось обработать запрос.",
            extensions: new Dictionary<string, object?>
            {
                ["traceId"] = traceId
            }).ExecuteAsync(httpContext);

        return true;
    }
}
