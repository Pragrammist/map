using System.Linq;
using MAP.Models;


namespace MAP.DbContexts
{
    // методы расширения, чтобы не писать LINQ запросы по милион раз
    
       
    
    // Класс сгенерирован Mapster. 
    // Нужен чтобы модели БД преобразовать в DTO
    // если планируется рефакторинг
    // то эти классы должны оноситься к деталям реализации работы с бд
    
    public static partial class PlaceMapper
    {
        public static PlaceDto AdaptToDto(this UsersAndPlacesContext.Place p1)
        {
            return p1 == null ? null : new PlaceDto()
            {
                Id = p1.Id,
                Name = p1.Name,
                Categories = p1.Categories == null 
                    ? Enumerable.Empty<string>() 
                    : p1.Categories.Select(c => c.Name),
                Info = p1.Info,
                Image = p1.Image,
                GeoJson = p1.GeoJson,
                LikeUserCount = p1.LikeUserCount,
                BlackListCount = p1.BlackListCount
            };
        }
        
        public static PlaceShortDto AdaptToShortDto(this UsersAndPlacesContext.Place p1)
        {
            return p1 == null ? null : new PlaceShortDto()
            {
                Id = p1.Id,
                Name = p1.Name,
                GeoJson = p1.GeoJson,
                LikeUserCount = p1.LikeUserCount,
                BlackListCount = p1.BlackListCount
            };
        }
        public static IEnumerable<PlaceShortDto> ProjectToShortDtos(
            this IEnumerable<UsersAndPlacesContext.Place> placesFromDb) => 
        placesFromDb.Select(s => s.AdaptToShortDto());
        
        
        
    }
}