using MAP.Models;
using MAP.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MAP.Controllers;

[ApiController]
[Route("place")]
public class PlaceController : ControllerBase
{
    [HttpGet()]
    public IActionResult GetPlace(string place)
    {
        return new ObjectResult(
            value: new PlaceDto {
                // я слишком ленив
            }
        );
    }

    [HttpGet("categories")]
    public IActionResult GetCategories(int take = 20, int page = 1)
    {
        return new ObjectResult(
            value: Enumerable.Empty<string>() // список названий категорий
        );
    }

    [HttpGet("{search}")]
    public IActionResult Search(string search)
    {
        return new ObjectResult(
            value: Enumerable.Empty<PlaceShortDto>() // список названий мест
        );
    }

    [HttpGet("category/{catecory}")]
    public IActionResult SearchByCategory(string catecory)
    {
        return new ObjectResult(
            value: Enumerable.Empty<PlaceShortDto>() // список названий мест
        );
    }

    [HttpGet("hot")]
    public IActionResult Hot()
    {
        return new ObjectResult(
            value: Enumerable.Empty<PlaceShortDto>() // список названий мест
        );
    }
}


