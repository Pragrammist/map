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
        var hashedPassword = await _hasher.Hash(password);
        var user = _context.Users.Include(u => u.BlackList).Include(u => u.LikedPlaces).FirstOrDefault(u => u.Login == login && u.Password == hashedPassword);

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
        var user = await _context.Users.Include(i => i.LikedPlaces).Include(u => u.BlackList).FirstAsync(u => u.Id == id);
        user.LikedPlaces = user.LikedPlaces ?? new List<UsersAndPlacesContext.Place>();
        

        if(user.LikedPlaces.Contains(place))
            return BadRequest("место уже добавлено");
        place.LikeUserCount++;
        user.LikedPlaces.Add(place);
        user.BlackList.Remove(place);
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
        var user = await _context.Users.Include(s => s.LikedPlaces).FirstAsync(u => u.Id == id);
        
        if(user.LikedPlaces is null)
            return NotFound();
        
        var place = user.LikedPlaces.FirstOrDefault(p => p.Id == placeId);

        if(place is null)
            return NotFound();
        
        user.LikedPlaces.Remove(place);

        if(place.LikeUserCount > 0)
            place.LikeUserCount--;
        
        await _context.SaveChangesAsync();
        return new ObjectResult(
            value: user.AdaptToDto()
        );
    }

    [HttpPut("blacklist")]
    [Authorize]
    public async Task<IActionResult> AddToBlacklistAsync(string placeId)
    {
        var id = User.FindFirstValue("id") ?? throw new HttpRequestException("id is null in identity");
        var place = await _context.Places.FirstAsync(p => p.Id == placeId);
        var user = await _context.Users.Include(i => i.BlackList).Include(u => u.LikedPlaces).FirstAsync(u => u.Id == id);
        user.BlackList = user.BlackList ?? new List<UsersAndPlacesContext.Place>();
        
        if(user.BlackList.Contains(place))
            return BadRequest("место уже добавлено");
        place.BlackListCount++;
        user.BlackList.Add(place);
        user.LikedPlaces.Remove(place);
        await _context.SaveChangesAsync();
        return new ObjectResult(
            value: user.AdaptToDto()
        );
    }
    [HttpPut("blacklist/delete")]
    [Authorize]
    public async Task<IActionResult> DeleteFromBlacklistAsync(string placeId)
    {
        var id = User.FindFirstValue("id") ?? throw new HttpRequestException("id is null in identity");
        var user = await _context.Users.Include(s => s.BlackList).FirstAsync(u => u.Id == id);
        
        if(user.BlackList is null)
            return NotFound();
        
        var place = user.BlackList.FirstOrDefault(p => p.Id == placeId);

        if(place is null)
            return NotFound();
        
        user.BlackList.Remove(place);

        if(place.BlackListCount > 0)
            place.BlackListCount--;
        
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


