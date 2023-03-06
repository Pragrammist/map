using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using MAP.DbContexts;
using MAP.Models;
using MAP.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MAP.Controllers;



[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    UsersAndPlacesContext _context;
    PasswordHasher _hasher;
    public UserController(UsersAndPlacesContext context, PasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
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
            Password = await _hasher.Hash(password),
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
    public async Task<IActionResult> AddPlaceAsync(string placeId)
    {
        var id = User.FindFirstValue("id") ?? throw new HttpRequestException("id is null in identity");
        var place = await _context.Places.FirstAsync(p => p.Id == placeId);
        var user = await _context.Users.Include(i => i.Places).FirstAsync(u => u.Id == id);
        user.Places = user.Places ?? new List<UsersAndPlacesContext.Place>();

        if(user.Places.Contains(place))
            return BadRequest("место уже добавлено");
        place.UserCount++;
        user.Places.Add(place);
        await _context.SaveChangesAsync();
        return new ObjectResult(
            value: user.AdaptToDto()
        );
    }
    [HttpPut("place/delete")]
    [Authorize]
    public async Task<IActionResult> DeletePlaceAsync(string placeId)
    {
        var id = User.FindFirstValue("id") ?? throw new HttpRequestException("id is null in identity");
        var user = await _context.Users.Include(s => s.Places).FirstAsync(u => u.Id == id);
        
        if(user.Places is null)
            return NotFound();
        
        var place = user.Places.FirstOrDefault(p => p.Id == placeId);

        if(place is null)
            return NotFound();
        
        user.Places.Remove(place);

        if(place.UserCount > 0)
            place.UserCount--;
        
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
        var user = await _context.Users.FindAsync(id) ?? throw new InvalidOperationException("id when updated email is not work");
        user.Email = email;
        await _context.SaveChangesAsync();
        return new ObjectResult(
            value: user.AdaptToDto()
        );
    }
}


