using Microsoft.AspNetCore.Identity;
using UserRegistration.Console.Models;

namespace UserRegistration.Console.Services;

public class UserService : IUserService
{
    private readonly List<User> _users = new();

    private readonly PasswordHasher<User> _passwordHasher = new();

    private int _nextId = 1;

    public User? Register(
        string login,
        string password,
        string firstName,
        string lastName)
    {
        var existingUser = _users.FirstOrDefault(
            u => u.Login.Equals(
                login,
                StringComparison.OrdinalIgnoreCase));

        if (existingUser != null)
        {
            return null;
        }

        var user = new User
        {
            Id = _nextId++,
            Login = login,
            FirstName = firstName,
            LastName = lastName
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            password);

        _users.Add(user);

        return user;
    }

    public User? Login(string login, string password)
    {
        var user = _users.FirstOrDefault(
            u => u.Login.Equals(
                login,
                StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password);

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

    public bool Update(
        int id,
        string firstName,
        string lastName)
    {
        var user = GetById(id);

        if (user == null)
        {
            return false;
        }

        user.FirstName = firstName;
        user.LastName = lastName;

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

    public List<User> GetAll()
    {
        return _users;
    }
}