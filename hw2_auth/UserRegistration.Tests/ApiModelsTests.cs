using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using UserRegistration.Api.Models;

namespace UserRegistration.Tests;

public class ApiModelsTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), errors, validateAllProperties: true);
        return errors;
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("  ", "password")]
    [InlineData("login", "")]
    [InlineData("login", "  ")]
    public void RegisterRequestRequiresLoginAndPassword(string login, string password)
    {
        var request = new RegisterRequest { Login = login, Password = password };
        Assert.NotEmpty(Validate(request));
    }

    [Fact]
    public void RegisterRequestAcceptsValidDataAndRejectsInvalidLengths()
    {
        var request = new RegisterRequest { Login = "student", Password = "Secret123", FirstName = "Anna", LastName = "Ivanova" };
        Assert.Empty(Validate(request));

        request.Login = new string('a', 101);
        request.Password = new string('b', 1025);
        var errors = Validate(request);
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(RegisterRequest.Login)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(RegisterRequest.Password)));
    }

    [Fact]
    public void RegisterRequestRejectsControlCharactersInLogin()
    {
        var request = new RegisterRequest { Login = "student\nname", Password = "Secret123" };
        Assert.Contains(Validate(request), error => error.MemberNames.Contains(nameof(RegisterRequest.Login)));
    }

    [Fact]
    public void DateFilterRejectsReversedRangeAndAllowsEqualBounds()
    {
        var date = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
        var query = new UsersQuery { From = date, To = date.AddDays(-1) };
        Assert.Contains(Validate(query), error => error.MemberNames.Contains(nameof(UsersQuery.From)));

        query.To = date;
        Assert.Empty(Validate(query));
    }

    [Theory]
    [InlineData("createdDate")]
    [InlineData("updatedDate")]
    [InlineData("CREATEDDATE")]
    public void DateFilterAcceptsSupportedFields(string field)
    {
        Assert.Empty(Validate(new UsersQuery { DateField = field }));
    }

    [Fact]
    public void DateFilterRejectsUnknownField()
    {
        var errors = Validate(new UsersQuery { DateField = "password" });
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(UsersQuery.DateField)));
    }

    [Fact]
    public void UserResponseDoesNotContainPasswordHash()
    {
        var user = new User { Id = 1, Login = "student", PasswordHash = "sensitive-hash", FirstName = "Anna" };
        var response = UserResponse.FromUser(user);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Equal(user.Id, response.Id);
        Assert.Equal(user.Login, response.Login);
        Assert.DoesNotContain("sensitive-hash", json);
        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);
    }
}
