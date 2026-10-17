using Microsoft.AspNetCore.Identity;
using UserRegistration.Api.Data;
using UserRegistration.Api.Models;

namespace UserRegistration.Api.Services;

public class UserService(UserRepository repository) : IUserService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public Task<User?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Login = request.Login.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            CreatedDate = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        return repository.AddAsync(user, cancellationToken);
    }

    public async Task<User?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await repository.FindByLoginAsync(request.Login.Trim(), cancellationToken);
        if (user == null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        return result == PasswordVerificationResult.Failed ? null : user;
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(id, cancellationToken);

    public Task<List<User>> GetAllAsync(UsersQuery query, CancellationToken cancellationToken) =>
        repository.GetAllAsync(query, cancellationToken);

    public Task<bool> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken) =>
        repository.UpdateAsync(
            id, request.FirstName.Trim(), request.LastName.Trim(), DateTime.UtcNow, cancellationToken);

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken) =>
        repository.DeleteAsync(id, cancellationToken);
}
