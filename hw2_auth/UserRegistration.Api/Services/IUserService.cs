using UserRegistration.Api.Models;

namespace UserRegistration.Api.Services;

public interface IUserService
{
    User? Register(RegisterRequest request);

    User? Login(LoginRequest request);

    User? GetById(int id);

    List<User> GetAll();

    bool Update(int id, UpdateUserRequest request);

    bool Delete(int id);
}