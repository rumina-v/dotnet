using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using UserRegistration.Api.Models;
using UserRegistration.Api.Services;

namespace UserRegistration.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(
        RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = await userService.RegisterAsync(request, cancellationToken);
        if (user == null)
        {
            return Conflict(new { message = "Пользователь с таким логином уже существует" });
        }

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, UserResponse.FromUser(user));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> GetById(
        [Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        return user == null
            ? NotFound(new { message = "Пользователь не найден" })
            : Ok(UserResponse.FromUser(user));
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetAll(
        [FromQuery] UsersQuery query, CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(query, cancellationToken);
        return Ok(users.Select(UserResponse.FromUser).ToList());
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        [Range(1, int.MaxValue)] int id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        return await userService.UpdateAsync(id, request, cancellationToken)
            ? NoContent()
            : NotFound(new { message = "Пользователь не найден" });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        [Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
    {
        return await userService.DeleteAsync(id, cancellationToken)
            ? NoContent()
            : NotFound(new { message = "Пользователь не найден" });
    }
}
