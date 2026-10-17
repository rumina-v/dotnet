using Approvals.Api.Data;
using Approvals.Api.Models;

namespace Approvals.Api.Services;

public class ApprovalService(RequestRepository repository, ILogger<ApprovalService> logger)
{
    public async Task<ApprovalRequest> CreateAsync(CreateRequest request, CancellationToken cancellationToken)
    {
        var created = await repository.CreateAsync(request, cancellationToken);
        logger.LogInformation(4001, "Создана заявка {RequestId} пользователя {UserId}.", created.Id, created.UserId);
        return created;
    }

    public Task<ApprovalRequest?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        repository.GetAsync(id, cancellationToken);

    public Task<List<ApprovalRequest>> ListAsync(int? userId, CancellationToken cancellationToken) =>
        repository.ListAsync(userId, cancellationToken);

    public async Task<DecisionResult> DecideAsync(Guid id, DecisionRequest decision, CancellationToken cancellationToken)
    {
        var updated = await repository.DecideAsync(id, decision.Status, cancellationToken);
        if (updated != null)
        {
            logger.LogInformation(4002, "Заявка {RequestId}: принято решение {Status}.", id, updated.Status);
            return new DecisionResult(updated, false);
        }

        var existing = await repository.GetAsync(id, cancellationToken);
        logger.LogWarning(4003, "Решение по заявке {RequestId} отклонено. Заявка существует: {Exists}.", id, existing != null);
        return new DecisionResult(existing, existing != null);
    }
}
