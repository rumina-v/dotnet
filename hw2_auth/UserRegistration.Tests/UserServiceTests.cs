using UserRegistration.Console.Services;

namespace UserRegistration.Tests;

public class UserServiceTests
{
    private readonly UserService service = new();

    [Fact]
    public void RegisterCreatesUserAndStoresPasswordHash()
    {
        var user = service.Register("anna", "Secret123", "Anna", "Ivanova");

        Assert.NotNull(user);
        Assert.True(user.Id > 0);
        Assert.Equal("anna", user.Login);
        Assert.Equal("Anna", user.FirstName);
        Assert.Equal("Ivanova", user.LastName);
        Assert.False(string.IsNullOrEmpty(user.PasswordHash));
        Assert.NotEqual("Secret123", user.PasswordHash);
        Assert.Same(user, service.GetById(user.Id));
    }

    [Fact]
    public void RegisterRejectsDuplicateLoginIgnoringCase()
    {
        var first = service.Register("Anna", "Secret123", "Anna", "Ivanova");
        var duplicate = service.Register("ANNA", "Other123", "Other", "User");

        Assert.NotNull(first);
        Assert.Null(duplicate);
        Assert.Single(service.GetAll());
        Assert.Same(first, service.Login("anna", "Secret123"));
    }

    [Fact]
    public void LoginRequiresCorrectPasswordAndExistingLogin()
    {
        var user = service.Register("student", "Secret123", "Anna", "Ivanova");

        Assert.NotNull(user);
        Assert.Same(user, service.Login("STUDENT", "Secret123"));
        Assert.Null(service.Login("student", "secret123"));
        Assert.Null(service.Login("unknown", "Secret123"));
    }

    [Fact]
    public void UpdateChangesNamesForTheRequestedUserOnly()
    {
        var first = service.Register("anna", "Secret123", "Anna", "Ivanova")!;
        var second = service.Register("maria", "Secret456", "Maria", "Petrova")!;

        Assert.True(service.Update(first.Id, "Anya", "Sidorova"));
        Assert.Equal("Anya", service.GetById(first.Id)!.FirstName);
        Assert.Equal("Sidorova", service.GetById(first.Id)!.LastName);
        Assert.Equal("Maria", service.GetById(second.Id)!.FirstName);
        Assert.False(service.Update(-1, "No", "User"));
        Assert.Same(first, service.Login("anna", "Secret123"));
    }

    [Fact]
    public void DeleteRemovesOnlySelectedUserAndPreventsLogin()
    {
        var first = service.Register("anna", "Secret123", "Anna", "Ivanova")!;
        var second = service.Register("maria", "Secret456", "Maria", "Petrova")!;

        Assert.True(service.Delete(first.Id));
        Assert.False(service.Delete(first.Id));
        Assert.Null(service.GetById(first.Id));
        Assert.Null(service.Login("anna", "Secret123"));
        Assert.Same(second, Assert.Single(service.GetAll()));
    }

    [Fact]
    public void EachServiceStartsWithAnEmptyUserList()
    {
        service.Register("anna", "Secret123", "Anna", "Ivanova");

        Assert.Empty(new UserService().GetAll());
    }
}
