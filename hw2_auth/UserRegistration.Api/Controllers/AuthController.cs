using Microsoft.AspNetCore.Mvc;
using UserRegistration.Api.Models;
using UserRegistration.Api.Services;

namespace UserRegistration.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IUserService userService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<UserResponse>> Login(
        LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userService.LoginAsync(request, cancellationToken);
        return user == null
            ? Unauthorized(new { message = "Неверный логин или пароль" })
            : Ok(UserResponse.FromUser(user));
    }
}
