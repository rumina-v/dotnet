using System.ComponentModel.DataAnnotations;

namespace UserRegistration.Api.Models;

public class RegisterRequest
{
    [Required]
    [MaxLength(100)]
    [RegularExpression(@"^[^\p{Cc}]*$")]
    public string Login { get; set; } = string.Empty;

    [Required]
    [MaxLength(1024)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(100)]
    [RegularExpression(@"^[^\p{Cc}]*$")]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    [RegularExpression(@"^[^\p{Cc}]*$")]
    public string LastName { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required]
    [MaxLength(100)]
    [RegularExpression(@"^[^\p{Cc}]*$")]
    public string Login { get; set; } = string.Empty;

    [Required]
    [MaxLength(1024)]
    public string Password { get; set; } = string.Empty;
}

public class UpdateUserRequest
{
    [MaxLength(100)]
    [RegularExpression(@"^[^\p{Cc}]*$")]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    [RegularExpression(@"^[^\p{Cc}]*$")]
    public string LastName { get; set; } = string.Empty;
}

public class UserResponse
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public static UserResponse FromUser(User user) => new()
    {
        Id = user.Id,
        Login = user.Login,
        FirstName = user.FirstName,
        LastName = user.LastName,
        CreatedDate = user.CreatedDate,
        UpdatedDate = user.UpdatedDate
    };
}
