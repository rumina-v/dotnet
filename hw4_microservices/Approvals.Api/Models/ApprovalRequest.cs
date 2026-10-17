using System.ComponentModel.DataAnnotations;

namespace Approvals.Api.Models;

public record ApprovalRequest(
    Guid Id, int UserId, string Title, string Description,
    string Status, DateTime CreatedDate, DateTime? DecidedDate);

public class CreateRequest
{
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = "";

    [Required, StringLength(2000)]
    public string Description { get; set; } = "";
}

public class DecisionRequest
{
    [Required, RegularExpression("^(approved|rejected)$")]
    public string Status { get; set; } = "";
}

public record DecisionResult(ApprovalRequest? Request, bool Conflict);
