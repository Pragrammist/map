using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using MAP.DbContexts;
using MAP.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
    public async Task<IActionResult> LoginAsync(string password, string login)
    {
        var user = _context.Users.FirstOrDefault(u => u.Login == login && u.Password == password);

        if(user is null)
            return NotFound();
        
        await Authenticate(login, user.Id);

        return new ObjectResult(
            value: user.AdaptToDto()
        );
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(string password, string login)
    {
        var user = await _context.Users.AddAsync(new UsersAndPlacesContext.User {
            Password = password,
            Login = login
        });
        await _context.SaveChangesAsync();
        await Authenticate(login, user.Entity.Id);
        return new ObjectResult(
            value: user.Entity.AdaptToDto()
        );
    }
    private async Task Authenticate(string login, string id)
    {
        var role = login == "admin" ? "admin" : "user";
        // создаем один claim
        var claims = new List<Claim>
        {
            new Claim(ClaimsIdentity.DefaultNameClaimType, login),
            new Claim("id", id),
            new Claim(ClaimsIdentity.DefaultRoleClaimType, role)
        };
        // создаем объект ClaimsIdentity
        ClaimsIdentity identity = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        // установка аутентификационных куки
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
    [HttpPut("place")]
    [Authorize]
    public async Task<IActionResult> AddPlaceAsync(string place)
    {
        var id = User.FindFirstValue("id") ?? throw new HttpRequestException("id is null in identity");
        var user = _context.Users.Update(new UsersAndPlacesContext.User {Id = id}).Entity;
        user.Places = user.Places ?? new List<UsersAndPlacesContext.Place>();
        user.Places.Add(new UsersAndPlacesContext.Place{Id = place});
        await _context.SaveChangesAsync();
        return new ObjectResult(
            value: user.AdaptToDto()
        );
    }
    [HttpPut("place/delete")]
    [Authorize]
    public async Task<IActionResult> DeletePlaceAsync(string place)
    {
        var id = User.FindFirstValue("id") ?? throw new HttpRequestException("id is null in identity");
        var user = _context.Users.Update(new UsersAndPlacesContext.User {Id = id}).Entity;
        if(user.Places is null)
            return NotFound();
        var userPlace = user.Places.FirstOrDefault(p => p.Id == place);
        if(userPlace is null)
            return NotFound();
        user.Places.Remove(userPlace);
        await _context.SaveChangesAsync();
        return new ObjectResult(
            value: user.AdaptToDto()
        );
    }

    [HttpPut("email")]
    [Authorize]
    public async Task<IActionResult> AddEmailAsync(string email)
    {
        var id = User.FindFirstValue("id") ?? throw new HttpRequestException("id is null in identity");
        var user = _context.Users.Update(new UsersAndPlacesContext.User {Id = id}).Entity;
        user.Email = email;
        await _context.SaveChangesAsync();
        return new ObjectResult(
            value: user.AdaptToDto()
        );
    }
}


