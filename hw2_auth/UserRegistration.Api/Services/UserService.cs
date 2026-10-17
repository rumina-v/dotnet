using Microsoft.AspNetCore.Identity;
using UserRegistration.Api.Data;
using UserRegistration.Api.Logging;
using UserRegistration.Api.Models;

namespace UserRegistration.Api.Services;

public class UserService(UserRepository repository, ILogger<UserService> logger) : IUserService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public async Task<User?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Login = request.Login.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            CreatedDate = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        var createdUser = await repository.AddAsync(user, cancellationToken);
        if (createdUser == null)
        {
            logger.LogWarning(LogEvents.DuplicateLogin, "Регистрация отклонена: логин уже занят.");
        }
        else
        {
            logger.LogInformation(LogEvents.UserCreated, "Создан пользователь {UserId}.", createdUser.Id);
        }
        return createdUser;
    }

    public async Task<User?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await repository.FindByLoginAsync(request.Login.Trim(), cancellationToken);
        if (user != null &&
            _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password)
                != PasswordVerificationResult.Failed)
        {
            logger.LogInformation(LogEvents.LoginSucceeded, "Выполнен вход пользователя {UserId}.", user.Id);
            return user;
        }

        logger.LogWarning(LogEvents.LoginFailed, "Вход отклонён: неверный логин или пароль.");
        return null;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            logger.LogWarning(LogEvents.UserNotFound,
                "Пользователь {UserId} не найден. Операция: {Operation}.", id, "read");
        }
        return user;
    }

    public async Task<List<User>> GetAllAsync(UsersQuery query, CancellationToken cancellationToken)
    {
        var users = await repository.GetAllAsync(query, cancellationToken);
        logger.LogInformation(LogEvents.UsersListed, "Получен список пользователей: {UserCount}.", users.Count);
        return users;
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var updated = await repository.UpdateAsync(
            id, request.FirstName.Trim(), request.LastName.Trim(), DateTime.UtcNow, cancellationToken);
        if (updated)
        {
            logger.LogInformation(LogEvents.UserUpdated, "Изменён пользователь {UserId}.", id);
        }
        else
        {
            logger.LogWarning(LogEvents.UserNotFound,
                "Пользователь {UserId} не найден. Операция: {Operation}.", id, "update");
        }
        return updated;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteAsync(id, cancellationToken);
        if (deleted)
        {
            logger.LogInformation(LogEvents.UserDeleted, "Удалён пользователь {UserId}.", id);
        }
        else
        {
            logger.LogWarning(LogEvents.UserNotFound,
                "Пользователь {UserId} не найден. Операция: {Operation}.", id, "delete");
        }
        return deleted;
    }
}
