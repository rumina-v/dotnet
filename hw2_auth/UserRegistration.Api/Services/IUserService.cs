using UserRegistration.Api.Models;

namespace UserRegistration.Api.Services;

public interface IUserService
{
    Task<User?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<User?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<User>> GetAllAsync(UsersQuery query, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
