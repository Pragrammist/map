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
[Route("places")]
public class PlaceController : ControllerBase
{
    bool IsAuth => User?.Identity?.IsAuthenticated ?? false;
    string UserId => User.FindFirstValue("id") ?? 
        throw new HttpRequestException("id is null in identity");
    readonly UsersAndPlacesContext _context;
    public PlaceController(UsersAndPlacesContext context)
    {
        _context = context;
    }
    /// <summary>
    /// Возвращает место по id
    /// </summary>
    /// <param name="id">айди</param>
    /// <returns>Полную информацию о месте</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlace(string id)
    {
        var placeFromDb = await _context.Places
            .Include(p => p.Categories)
            .FindByIdAsync(id);
        if(placeFromDb is null)
            return NotFound();
        var placeDto = placeFromDb.AdaptToDto();
        return new ObjectResult(
            value: placeDto
        );
    }

    /// <summary>
    /// Возвращает все места
    /// </summary>
    /// <returns>Полную информацию о месте</returns>
    [HttpGet]
    public async Task<IActionResult> GetPlaces()
    {
        var places =  await _context.Places
            .Include(p => p.Categories)
            .Select(s => s.AdaptToDto()).ToListAsync();
        
        
        return new ObjectResult(
            value: places
        );
    }
    /// <summary>
    /// Возвращает категории которые есть
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="take">количество</param>
    /// <param name="page">страница(смещение по бд)</param>
    /// <returns>Массив категорий</returns>
    [HttpGet("categories")]
    public IActionResult GetCategories(int take = 20, int page = 0)
    {
        var categories = _context.Categories
            .TakeByPage(page, take)
            .SelectCategoriesByName();
        return new ObjectResult(categories);
    }

    /// <summary>
    /// поиск места по имени
    /// </summary>
    /// <remarks>
    /// если залогиниться и добавить места в черный список, они не будут отображаться в результате
    /// </remarks>
    /// <param name="search">строка поиска</param>
    /// <param name="take">количество</param>
    /// <param name="page">страница(смещение по бд)</param>
    /// <returns>массив мест(краткая версия места)</returns>
    [HttpGet("{search}")]
    public async Task<IActionResult> Search(string search, int page = 0, int take = 20)
    {
        var placesFilteredByQuery = _context.Places.WhereByName(search);
        var blacklistFilter = await BlackListFilter(placesFilteredByQuery);
        var orderedByRating = blacklistFilter.OrderByRatingAndTakeFromPage(page, take);
        return GetManyPlacesObjectResult(orderedByRating);
        
    }
    
    /// <summary>
    /// поиск по категории
    /// </summary>
    /// <remarks>
    /// если залогиниться и добавить места в черный список, они не будут отображаться в результате
    /// </remarks>
    /// <param name="category">категория</param>
    /// <param name="take">количество</param>
    /// <param name="page">страница(смещение по бд)</param>
    /// <returns>массив мест(краткая версия места)</returns>
    [HttpGet("category/{category}")]
    public async Task<IActionResult> SearchByCategoryAsync(string category, int page = 0, int take = 20)
    {
        var placesFilteredByCategory = _context.Places
            .Include(p => p.Categories)
            .WhereByCategory(category);
        var blacklistFilter = await BlackListFilter(placesFilteredByCategory);
        var orderedByRating = blacklistFilter.OrderByRatingAndTakeFromPage(page, take);
        return GetManyPlacesObjectResult(orderedByRating);
    }
    /// <summary>
    /// возвращает горячие места
    /// </summary>
    /// <remarks>
    /// если залогинить и добавить места в черный список, они не будут отображаться в результате
    /// </remarks>
    /// <param name="take">количество</param>
    /// <param name="page">страница(смещение по бд)</param>
    /// <returns>массив мест(краткая версия места)</returns>
    [HttpGet("hot")]
    public async Task<IActionResult> HotAsync(int page = 0, int take = 20)
    {
        const string HOT_CATEGORY = "hot";
        var placesFilteredByHotCategory = _context.Places
            .Include(p => p.Categories)
            .WhereByCategory(HOT_CATEGORY);
        var blacklistFilter = await BlackListFilter(placesFilteredByHotCategory);
        var orderedByRating = blacklistFilter.OrderByRatingAndTakeFromPage(page, take);
        return GetManyPlacesObjectResult(orderedByRating);
    }
    /// <summary>
    /// Добавление категории. Создает новую если такой нет
    /// </summary>
    /// <remarks>
    /// чтобы воспользоваться нужна залогиниться с логином admin, и его паролем.
    /// изначально админа нет, его нужно зарегестрировать с логином admin
    /// </remarks>
    /// <param name="categoryName">название категории</param>
    /// <param name="placeId">айди категории</param>
    /// <returns>Полную информацию о месте</returns>
    [HttpPost("categories")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AddCategory(string categoryName, string placeId)
    {
        var place = await _context.Places
            .Include(c => c.Categories)
            .FindByIdAsync(placeId);
        
        if(place is null)
            return NotFound("place not found");

        var category = await _context.Categories.FindByNameAsync(categoryName);

        if(category is null)
            category = new UsersAndPlacesContext.Category { Name = categoryName};
        
        if(place.Categories.Contains(category))
            return BadRequest("category already exists");

        place.Categories.Add(category);

        await _context.SaveChangesAsync();
        
        return new ObjectResult(place.AdaptToDto());
    }

    /// <summary>
    /// удаление категории из места
    /// </summary>
    /// <remarks>
    /// чтобы воспользоваться нужна залогиниться с логином admin, и его паролем.
    /// изначально админа нет, его нужно зарегестрировать с логином admin
    /// </remarks>
    /// <param name="categoryName">название категории</param>
    /// <param name="placeId">айди категории</param>
    /// <returns>Полную информацию о месте</returns>
    [HttpPut("categories")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteCategory(string categoryName, string placeId)
    {
        var place = await _context.Places
            .Include(c => c.Categories)
            .FindByIdAsync(placeId);
        
        if(place is null)
            return NotFound("place not found");

        var category = await _context.Categories.FindByNameAsync(categoryName);

        if(category is null)
            return NotFound("category not found");
        
        place.Categories.Remove(category);

        await _context.SaveChangesAsync();

        return new ObjectResult(place);
    }

    /// <summary>
    /// создание места
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// Пример заполения геоданных(формат геоданных может быть любой, какой удобен)
    /// {
    ///     ...
    ///     geoJson: {
    ///         "type": "Feature",
    ///         "geometry": {
    ///             "type": "Point",
    ///             "coordinates": [125.6, 10.1]
    ///         },
    ///         "properties": {
    ///             "name": "Dinagat Islands"
    ///         }
    ///     }
    ///     ...
    /// }
    /// Прежде чем добавить фото к месту, это фото нужно загрузить на сервер.
    /// При отправке нового объекта места нужно передать название файла, которое возвращает сервер 
    /// чтобы воспользоваться нужна залогиниться с логином admin, и его паролем.
    /// изначально админа нет, его нужно зарегестрировать с логином admin
    /// </remarks>
    /// <param name="createPlaceDto">инфа о месте</param>
    /// <returns>Полную информацию о месте</returns>
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create(CreatePlaceDto createPlaceDto)
    {
        var place = await createPlaceDto.AdaptToPlace(_context);
        
        var res = await  _context.Places.AddAsync(place);

        await  _context.SaveChangesAsync();

        return new ObjectResult(
            value: res.Entity.AdaptToShortDto()
        );
        
    }


    IActionResult GetManyPlacesObjectResult(IEnumerable<UsersAndPlacesContext.Place> placesFromDb)
    {
        return new ObjectResult(
            value: placesFromDb.ProjectToShortDtos() // список названий мест
        );
    }
    async Task<IEnumerable<UsersAndPlacesContext.Place>> BlackListFilter(
        IEnumerable<UsersAndPlacesContext.Place> placesFromDb)
    {
        if(!IsAuth)
            return placesFromDb;
        
        var user = await _context.Users.Include(u => u.BlackList).FindByIdAsync(UserId);

        var blacklist = user.BlackList.SelectById();

        var filterByBlacklist = placesFromDb.FilterByBlackList(blacklist);

        return filterByBlacklist;
    }
}


