using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using UserRegistration.Api.Models;
using UserRegistration.Api.Services;

namespace UserRegistration.Api.Controllers;

[ApiController]
[Route("api/requests")]
public class RequestsController(
    IUserService userService, ApprovalServiceClient approvals, ILogger<RequestsController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateApprovalRequest request, CancellationToken cancellationToken)
    {
        if (await userService.GetByIdAsync(request.UserId, cancellationToken) == null)
        {
            return NotFound(new { message = "Пользователь не найден." });
        }
        return await ForwardAsync(HttpMethod.Post, "api/requests", request, cancellationToken);
    }

    [HttpGet]
    public Task<IActionResult> List(
        [FromQuery, Range(1, int.MaxValue)] int? userId, CancellationToken cancellationToken) =>
        ForwardAsync(HttpMethod.Get, userId.HasValue ? $"api/requests?userId={userId.Value}" : "api/requests",
            null, cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        ForwardAsync(HttpMethod.Get, $"api/requests/{id}", null, cancellationToken);

    [HttpPut("{id:guid}/decision")]
    public Task<IActionResult> Decide(Guid id, ApprovalDecisionRequest decision, CancellationToken cancellationToken) =>
        ForwardAsync(HttpMethod.Put, $"api/requests/{id}/decision", decision, cancellationToken);

    private async Task<IActionResult> ForwardAsync(
        HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        try
        {
            var reply = await approvals.SendAsync(method, path, body, cancellationToken);
            if (reply.Location != null) { Response.Headers.Location = reply.Location; }
            return new ContentResult { StatusCode = (int)reply.StatusCode, ContentType = "application/json", Content = reply.Json };
        }
        catch (HttpRequestException)
        {
            return Unavailable();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Unavailable();
        }
    }

    private ObjectResult Unavailable()
    {
        logger.LogWarning(5001, "Сервис согласования заявок недоступен.");
        return Problem(statusCode: 503, title: "Сервис согласования заявок временно недоступен.");
    }
}
