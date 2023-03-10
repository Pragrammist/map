using MAP.DbContexts;
using MAP.Models;
using Microsoft.AspNetCore.Mvc;
using Mapster;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MAP.Controllers;

[ApiController]
[Route("place")]
public class PlaceController : ControllerBase
{
    readonly UsersAndPlacesContext _context;
    public PlaceController(UsersAndPlacesContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> GetPlace(string id)
    {
        var placeFromDb = await _context.Places.Include(p => p.Categories).FirstAsync(p => p.Id == id);
        if(placeFromDb is null)
            return NotFound();
        var placeDto = placeFromDb.AdaptToDto();
        return new ObjectResult(
            value: placeDto
        );
    }

    [HttpGet("categories")]
    public IActionResult GetCategories(int take = 20, int page = 1)
    {
        var categories = _context.Categories.Skip(take * (page - 1)).Take(20).Select(c => c.Name);
        return new ObjectResult(
            value: categories // список названий категорий
        );
    }
    bool IsAuth => User?.Identity?.IsAuthenticated ?? false;
    [HttpGet("{search}")]
    public async Task<IActionResult> Search(string search)
    {
        var placesFilteredByQuery = _context.Places
            .Where(p => search.ToLower().Contains(p.Name) || 
                    p.Name.ToLower().Contains(search));

        return IsAuth ? await BlackListFilter(placesFilteredByQuery) : GetObjectResult(placesFilteredByQuery);
        
    }
    IActionResult GetObjectResult(IQueryable<UsersAndPlacesContext.Place> placesFromDb)
    {
        return new ObjectResult(
            value: placesFromDb.Select(s => s.AdaptToShortDto()) // список названий мест
        );
    }
    async  Task<IActionResult> BlackListFilter(IQueryable<UsersAndPlacesContext.Place> placesFromDb)
    {
        var userId = User.FindFirstValue("id") ?? throw new HttpRequestException("id is null in identity");
        var user = await _context.Users.Include(u => u.BlackList).FirstAsync(u => u.Id == userId);

        var blacklist = user.BlackList.Select(u => u.Id);

        var filterByBlacklist = placesFromDb.Where(p => !blacklist.Contains(p.Id));

        return GetObjectResult(filterByBlacklist);
    }
    [HttpGet("category/{category}")]
    public async Task<IActionResult> SearchByCategoryAsync(string category)
    {
        var placesFilteredByCategory = _context.Places.Where(p => p.Categories.FirstOrDefault(c => c.Name.ToLower() == category.ToLower()) != null);
        return IsAuth ? await BlackListFilter(placesFilteredByCategory) : GetObjectResult(placesFilteredByCategory);
    }

    [HttpGet("hot")]
    public async Task<IActionResult> HotAsync()
    {
        const string HOT_CATEGORY = "hot";
        var placesFilteredByHotCategory = _context.Places.Where(p => p.Categories.FirstOrDefault(c => c.Name.ToLower() == HOT_CATEGORY) != null);
        return IsAuth ? await BlackListFilter(placesFilteredByHotCategory) : GetObjectResult(placesFilteredByHotCategory);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create(CreatePlaceDto createPlaceDto)
    {
        var place = createPlaceDto.AdaptToPlace();
        
        var res = await  _context.Places.AddAsync(place);

        await  _context.SaveChangesAsync();

        return new ObjectResult(
            value: res.Entity.AdaptToShortDto()
        );
        
    }
}


