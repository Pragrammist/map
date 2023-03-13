using MAP.DbContexts;
using MAP.Models;
using Microsoft.AspNetCore.Mvc;
using Mapster;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static MAP.DbContexts.LINQHelpers;

namespace MAP.Controllers;

[ApiController]
[Route("place")]
public class PlaceController : ControllerBase
{
    bool IsAuth => User?.Identity?.IsAuthenticated ?? false;
    string UserId => User.FindFirstValue("id") ?? throw new HttpRequestException("id is null in identity");
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
    public IActionResult GetCategories(int take = 20, int page = 0)
    {
        var categories = _context.Categories.Skip(take * page).Take(take).Select(c => c.Name);
        return new ObjectResult(
            value: categories // список названий категорий
        );
    }

    [HttpGet("{search}")]
    public async Task<IActionResult> Search(string search, int page = 0, int take = 20)
    {
        var placesFilteredByQuery = _context.Places
            .Where(p => search.ToLower().Contains(p.Name) || 
                    p.Name.ToLower().Contains(search));
        var blacklistFilter = await BlackListFilter(placesFilteredByQuery);
        var orderedByRating = blacklistFilter.OrderByRatingAndTakeFromPage(page, take);
        return GetObjectResult(orderedByRating);
        
    }
    
    
    [HttpGet("category/{category}")]
    public async Task<IActionResult> SearchByCategoryAsync(string category, int page = 0, int take = 20)
    {
        var placesFilteredByCategory = _context.Places.Where(p => p.Categories.FirstOrDefault(c => c.Name.ToLower() == category.ToLower()) != null).OrderByRatingAndTakeFromPage(page, take);
        var blacklistFilter = await BlackListFilter(placesFilteredByCategory);
        var orderedByRating = blacklistFilter.OrderByRatingAndTakeFromPage(page, take);
        return GetObjectResult(orderedByRating);
    }

    [HttpGet("hot")]
    public async Task<IActionResult> HotAsync(int page = 0, int take = 20)
    {
        const string HOT_CATEGORY = "hot";
        var placesFilteredByHotCategory = _context.Places.Where(p => p.Categories.FirstOrDefault(c => c.Name.ToLower() == HOT_CATEGORY) != null).OrderByRatingAndTakeFromPage(page, take);
        var blacklistFilter = await BlackListFilter(placesFilteredByHotCategory);
        var orderedByRating = blacklistFilter.OrderByRatingAndTakeFromPage(page, take);
        return GetObjectResult(orderedByRating);
    }
    [HttpPost("categories")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AddCategory(string categoryName, string placeId)
    {
        var place = await _context.Places.Include(c => c.Categories).FirstOrDefaultAsync(p => p.Id == placeId);
        if(place is null)
            return NotFound("place not found");

        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == categoryName);

        if(category is null)
            category = new UsersAndPlacesContext.Category { Name = categoryName};
        
        place.Categories.Add(category);
        await _context.SaveChangesAsync();

        return new ObjectResult(place.AdaptToDto());
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


    IActionResult GetObjectResult(IEnumerable<UsersAndPlacesContext.Place> placesFromDb)
    {
        return new ObjectResult(
            value: placesFromDb.Select(s => s.AdaptToShortDto()) // список названий мест
        );
    }
    async  Task<IEnumerable<UsersAndPlacesContext.Place>> BlackListFilter(IEnumerable<UsersAndPlacesContext.Place> placesFromDb)
    {
        if(!IsAuth)
            return placesFromDb;
        
        var user = await _context.Users.Include(u => u.BlackList).FirstAsync(u => u.Id == UserId);

        var blacklist = user.BlackList.Select(u => u.Id);

        var filterByBlacklist = placesFromDb.Where(p => !blacklist.Contains(p.Id));

        return filterByBlacklist;
    }
}


