using MAP.DbContexts;
using MAP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MAP.Controllers;



[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    UsersAndPlacesContext _context;
    public UserController(UsersAndPlacesContext context)
    {
        _context = context;
    }
    [HttpPost("login")]
    public IActionResult Login(string password, string login)
    {
        return new ObjectResult(
            value: new UserDto { 
                Email = "",
                Name = "",
            }
        );
    }

    [HttpPost("register")]
    public IActionResult Register(string password, string login)
    {
        return new ObjectResult(
            value: new UserDto { 
                Email = "",
                Name = "",
            }
        );
    }

    [HttpPost("place")]
    [Authorize]
    public IActionResult AddPlace(string login, string place)
    {
        return new ObjectResult(
            value: new UserDto { 
                Email = "",
                Name = "",
                
            }
        );
    }


    [HttpPost("email")]
    [Authorize]
    public IActionResult AddEmail(string login, string password, string email)
    {
        return new ObjectResult(
            value: new UserDto { 
                Email = "",
                Name = "",
            }
        );
    }
}


