using MAP.DbContexts;
using MAP.Models;
using MAP.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Mapster;

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
    public async Task<IActionResult> GetPlace(string place)
    {
        var placeFromDb = await _context.Places.FindAsync(place);
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

    [HttpGet("{search}")]
    public IActionResult Search(string search)
    {
        var placesFromDb = _context.Places
            .Where(p => search.ToLower().Contains(p.Name) || 
                    p.Name.ToLower().Contains(search));
        return new ObjectResult(
            value: placesFromDb.Select(s => s.AdaptToShortDto()) // список названий мест
        );
    }

    [HttpGet("category/{category}")]
    public IActionResult SearchByCategory(string category)
    {
        var placesFromDb = _context.Places.Where(p => p.Categories.FirstOrDefault(c => c.Name == category) != null);
        return new ObjectResult(
            value: placesFromDb.Select(s => s.AdaptToShortDto()) // список названий мест
        );
    }

    [HttpGet("hot")]
    public IActionResult Hot()
    {
        const string HOT_CATEGORY = "hot";
        var placesFromDb = _context.Places.Where(p => p.Categories.FirstOrDefault(c => c.Name == HOT_CATEGORY) != null);
        return new ObjectResult(
            value: placesFromDb.Select(s => s.AdaptToShortDto()) // список названий мест
        );
    }

    [HttpPost]
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


