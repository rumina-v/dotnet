using Microsoft.AspNetCore.Mvc;
using UserRegistration.Api.Models;
using UserRegistration.Api.Services;

namespace UserRegistration.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public ActionResult<UserResponse> Login(
        LoginRequest request)
    {
        var user = _userService.Login(request);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Неверный логин или пароль"
            });
        }

        return Ok(new UserResponse
        {
            Id = user.Id,
            Login = user.Login,
            FirstName = user.FirstName,
            LastName = user.LastName
        });
    }
}