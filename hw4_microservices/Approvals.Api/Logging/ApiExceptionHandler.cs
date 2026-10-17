using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;

namespace Approvals.Api.Logging;

public class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        logger.LogError(4004, "Ошибка сервиса заявок. Тип: {ExceptionType}. TraceId: {TraceId}.",
            exception.GetType().Name, traceId);
        await Results.Problem(statusCode: 500, title: "Не удалось обработать заявку.",
            extensions: new Dictionary<string, object?> { ["traceId"] = traceId }).ExecuteAsync(context);
        return true;
    }
}
