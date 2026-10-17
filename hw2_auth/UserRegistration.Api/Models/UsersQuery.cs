using System.ComponentModel.DataAnnotations;

namespace UserRegistration.Api.Models;

public class UsersQuery : IValidatableObject
{
    [Required]
    [RegularExpression("^(?i:createdDate|updatedDate)$")]
    public string DateField { get; set; } = "createdDate";

    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (From.HasValue && To.HasValue && From > To)
        {
            yield return new ValidationResult(
                "Начало периода не может быть позже конца.",
                [nameof(From), nameof(To)]);
        }
    }
}
