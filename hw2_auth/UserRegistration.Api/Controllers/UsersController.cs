using Microsoft.AspNetCore.Mvc;
using UserRegistration.Api.Models;
using UserRegistration.Api.Services;

namespace UserRegistration.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public ActionResult<UserResponse> Create(
        RegisterRequest request)
    {
        var user = _userService.Register(request);

        if (user == null)
        {
            return BadRequest(new
            {
                message =
                    "Пользователь с таким логином уже существует или данные некорректны"
            });
        }

        var response = new UserResponse
        {
            Id = user.Id,
            Login = user.Login,
            FirstName = user.FirstName,
            LastName = user.LastName
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = user.Id },
            response);
    }

    [HttpGet("{id:int}")]
    public ActionResult<UserResponse> GetById(int id)
    {
        var user = _userService.GetById(id);

        if (user == null)
        {
            return NotFound(new
            {
                message = "Пользователь не найден"
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

    [HttpGet]
    public ActionResult<List<UserResponse>> GetAll()
    {
        var users = _userService.GetAll();

        var response = users.Select(user => new UserResponse
        {
            Id = user.Id,
            Login = user.Login,
            FirstName = user.FirstName,
            LastName = user.LastName
        }).ToList();

        return Ok(response);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(
        int id,
        UpdateUserRequest request)
    {
        var result = _userService.Update(id, request);

        if (!result)
        {
            return NotFound(new
            {
                message = "Пользователь не найден"
            });
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var result = _userService.Delete(id);

        if (!result)
        {
            return NotFound(new
            {
                message = "Пользователь не найден"
            });
        }

        return NoContent();
    }
}