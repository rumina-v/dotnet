using Microsoft.AspNetCore.Identity;
using UserRegistration.Api.Models;

namespace UserRegistration.Api.Services;

public class UserService : IUserService
{
    private readonly List<User> _users = new();

    private readonly PasswordHasher<User> _passwordHasher = new();

    private int _nextId = 1;

    public User? Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var existingUser = _users.FirstOrDefault(
            u => u.Login.Equals(
                request.Login,
                StringComparison.OrdinalIgnoreCase));

        if (existingUser != null)
        {
            return null;
        }

        var user = new User
        {
            Id = _nextId++,
            Login = request.Login,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password);

        _users.Add(user);

        return user;
    }

    public User? Login(LoginRequest request)
    {
        var user = _users.FirstOrDefault(
            u => u.Login.Equals(
                request.Login,
                StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (result == PasswordVerificationResult.Success ||
            result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            return user;
        }

        return null;
    }

    public User? GetById(int id)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }

    public List<User> GetAll()
    {
        return _users;
    }

    public bool Update(
        int id,
        UpdateUserRequest request)
    {
        var user = GetById(id);

        if (user == null)
        {
            return false;
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        return true;
    }

    public bool Delete(int id)
    {
        var user = GetById(id);

        if (user == null)
        {
            return false;
        }

        _users.Remove(user);

        return true;
    }
}