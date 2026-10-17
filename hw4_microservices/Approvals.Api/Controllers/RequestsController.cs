using System.ComponentModel.DataAnnotations;
using Approvals.Api.Models;
using Approvals.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Approvals.Api.Controllers;

[ApiController]
[Route("api/requests")]
public class RequestsController(ApprovalService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApprovalRequest>> Create(CreateRequest request, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApprovalRequest>> Get(Guid id, CancellationToken cancellationToken)
    {
        var request = await service.GetAsync(id, cancellationToken);
        return request == null ? NotFound(new { message = "Заявка не найдена." }) : Ok(request);
    }

    [HttpGet]
    public async Task<ActionResult<List<ApprovalRequest>>> List(
        [FromQuery, Range(1, int.MaxValue)] int? userId, CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(userId, cancellationToken));

    [HttpPut("{id:guid}/decision")]
    public async Task<ActionResult<ApprovalRequest>> Decide(
        Guid id, DecisionRequest decision, CancellationToken cancellationToken)
    {
        var result = await service.DecideAsync(id, decision, cancellationToken);
        if (result.Conflict) { return Conflict(new { message = "По заявке уже принято решение." }); }
        return result.Request == null ? NotFound(new { message = "Заявка не найдена." }) : Ok(result.Request);
    }
}
