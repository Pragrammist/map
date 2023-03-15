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
    bool IsAuth => User?.Identity?.IsAuthenticated ?? false;
    string UserId => User.FindFirstValue("id") ?? 
        throw new HttpRequestException("id is null in identity");

    readonly UsersAndPlacesContext _context;
    readonly PasswordHasher _hasher;
    public UserController(UsersAndPlacesContext context, PasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }
    /// <summary>
    /// логинит на на сервере пользователя
    /// </summary>
    /// <remarks>
    /// чтобы воспользоваться нужна залогиниться с логином admin, и его паролем.
    /// изначально админа нет, его нужно зарегестрировать с логином admin
    /// </remarks>
    /// <param name="password">пароль</param>
    /// <param name="login">логин</param>
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(string password, string login)
    {
        var hashedPassword = await _hasher.Hash(password);
        var user = await _context.Users
            .Include(u => u.BlackList)
            .Include(u => u.LikedPlaces)
            .FindUserByLoginAndPassHash(login, hashedPassword);

        if(user is null)
            return NotFound();
        
        await Authenticate(login, user.Id);

        return new ObjectResult(user.AdaptToDto());
    }
    /// <summary>
    /// регестрация новго пользователя
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="password">пароль</param>
    /// <param name="login">логин</param>
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

    /// <summary>
    /// добавляет место пользователю
    /// </summary>
    /// <remarks>
    /// если место есть в черном списке, то оно от туда удаляется
    /// </remarks>
    /// <param name="placeId">айдми место</param>
    [HttpPut("place")]
    [Authorize]
    public async Task<IActionResult> AddPlaceAsync(string placeId)
    {
        var place = await _context.Places
            .FindAsync(placeId);

        var user = await _context.Users
            .Include(i => i.LikedPlaces)
            .Include(u => u.BlackList)
            .FindByIdAsync(UserId);
        
        if(place is null)
            return BadRequest("место не найдено");

        if(!AddLikedPlaceLogic(place, user))
            return BadRequest("место уже добавлено");
        
        
        
        
        await _context.SaveChangesAsync();

        return new ObjectResult(
            value: user.AdaptToDto()
        );
    }

    /// <summary>
    /// удаление место из любимых
    /// </summary>
    /// <remarks>
    /// НЕ добавляет место в черный список
    /// </remarks>
    /// <param name="placeId">айдми место</param>
    [HttpPut("place/delete")]
    [Authorize]
    public async Task<IActionResult> DeletePlaceAsync(string placeId)
    {
        var user = await _context.Users
            .Include(s => s.LikedPlaces)
            .FindByIdAsync(UserId);
        
        
        if(!DeleteLikedPlaceLogic(user, placeId))
            return NotFound();
        
        await _context.SaveChangesAsync();


        return new ObjectResult(user.AdaptToDto());
    }
    
    /// <summary>
    /// добавление места в черный список
    /// </summary>
    /// <remarks>
    /// удаляет место из любимых если такой
    /// </remarks>
    /// <param name="placeId">айдми место</param>
    [HttpPut("blacklist")]
    [Authorize]
    public async Task<IActionResult> AddToBlacklistAsync(string placeId)
    {
        var place = await _context.Places.FindAsync(placeId);

        var user = await _context.Users
            .Include(i => i.BlackList)
            .Include(u => u.LikedPlaces)
            .FindByIdAsync(UserId);

        
        if(place is null)
            return BadRequest("место не найдено");

        if(!AddToBlackListLogic(place, user))
            return BadRequest("место уже добавлено");
        

        await _context.SaveChangesAsync();

        return new ObjectResult(
            value: user.AdaptToDto()
        );
    }
    
    /// <summary>
    /// удаление места из черного списка
    /// </summary>
    /// <remarks>
    /// НЕ добавляет место в любимое
    /// </remarks>
    /// <param name="placeId">айдми место</param>
    [HttpPut("blacklist/delete")]
    [Authorize]
    public async Task<IActionResult> DeleteFromBlacklistAsync(string placeId)
    {
        var user = await _context.Users.Include(s => s.BlackList).FindByIdAsync(UserId);
        
        if(!DeleteFromBlackListLogic(user, placeId))
            return NotFound();
        
        await _context.SaveChangesAsync();

        return new ObjectResult(
            value: user.AdaptToDto()
        );
    }
    
    /// <summary>
    /// меняет или добавляет новую почту
    /// </summary>
    /// <remarks>
    /// айди берется из кук
    /// </remarks>
    /// <param name="email">новая почта</param>
    [HttpPut("email")]
    [Authorize]
    public async Task<IActionResult> AddEmailAsync(string email)
    {
        var user = await _context.Users.FindByIdAsync(UserId);
        user.Email = email;
        await _context.SaveChangesAsync();
        return new ObjectResult(
            value: user.AdaptToDto()
        );
    }

    bool AddLikedPlaceLogic(UsersAndPlacesContext.Place place, UsersAndPlacesContext.User user)
    {
        //бизнес логика
        //по желанию переместить в другое место

        //когда добавляется место увеличивается счетчик
        //после добавления места производится попытка
        //удаления места из черного списка
        //если таково имеется

        //если место уже есть то ничего не делается
        //чтобы счетчик зря не трогать
        
       
        if(user.LikedPlaces.Contains(place))
            return false;
        place.LikeUserCount++;
        user.LikedPlaces.Add(place);

        DeleteFromBlackListLogic(user, place.Id);
        return true;
    }
    bool DeleteLikedPlaceLogic(UsersAndPlacesContext.User user, string placeId)
    {
        //бизнес логика
        //по желанию переместить в другое место


        var placeFromLikedList = user.LikedPlaces.FindById(placeId);


        if(placeFromLikedList is null)
            return false;


        //удаляется место счетчик уменьщается
        user.LikedPlaces.Remove(placeFromLikedList);

        if(placeFromLikedList.LikeUserCount > 0)
            placeFromLikedList.LikeUserCount--;

        return true;
    }
    bool AddToBlackListLogic(UsersAndPlacesContext.Place place, UsersAndPlacesContext.User user)
    {
        //бизнес логика
        //по желанию переместить в другое место
        // при добавлении какого

        //все тоже самое как с любимыми местами но наоборот
        if(user.BlackList.Contains(place))
            return false;

        place.BlackListCount++;
        user.BlackList.Add(place);

        DeleteLikedPlaceLogic(user, place.Id);
        return true;
    }
    bool DeleteFromBlackListLogic(UsersAndPlacesContext.User user, string placeId)
    {
        //бизнес логика
        //по желанию переместить в другое место

        //логика такая же как у любимых мест
        var blackListPlace = user.BlackList.FindById(placeId);

        if(blackListPlace is null)
            return false;
        
        user.BlackList.Remove(blackListPlace);

        if(blackListPlace.BlackListCount > 0)
            blackListPlace.BlackListCount--;

        return true;
    }
    
    private async Task Authenticate(string login, string id)
    {
        var role = login == "admin" 
            ? "admin" 
            : "user";
        // создаем один claim
        var claims = new List<Claim>
        {
            new Claim(ClaimsIdentity.DefaultNameClaimType, login),
            new Claim("id", id),
            new Claim(ClaimsIdentity.DefaultRoleClaimType, role)
        };
        // создаем объект ClaimsIdentity
        ClaimsIdentity identity = new ClaimsIdentity(
            claims, 
            "ApplicationCookie",
            ClaimsIdentity.DefaultNameClaimType, 
            ClaimsIdentity.DefaultRoleClaimType
        );
        // установка аутентификационных куки
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, 
            new ClaimsPrincipal(identity)
        );
    }
}


