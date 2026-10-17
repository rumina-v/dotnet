using System.ComponentModel.DataAnnotations;

namespace UserRegistration.Api.Models;

public class CreateApprovalRequest
{
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = "";

    [Required, StringLength(2000)]
    public string Description { get; set; } = "";
}

public class ApprovalDecisionRequest
{
    [Required, RegularExpression("^(approved|rejected)$")]
    public string Status { get; set; } = "";
}
