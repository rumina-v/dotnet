using UserRegistration.Console.Models;

namespace UserRegistration.Console.Services;

public interface IUserService
{
    User? Register(
        string login,
        string password,
        string firstName,
        string lastName);

    User? Login(string login, string password);

    User? GetById(int id);

    bool Update(
        int id,
        string firstName,
        string lastName);

    bool Delete(int id);

    List<User> GetAll();
}