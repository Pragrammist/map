using System.Linq;
using MAP.Models;

namespace MAP.DbContexts
{
    // Класс сгенерирован Mapster. 
    // Нужен чтобы модели БД преобразовать в DTO
    // если планируется рефакторинг
    // то эти классы должны оноситься к деталям реализации работы с бд
    
    public static partial class UserMapper
    {
        public static UserDto AdaptToDto(this UsersAndPlacesContext.User p1)
        {
            return p1 == null ? null : new UserDto()
            {
                Id = p1.Id,
                LikedPlaces = p1.LikedPlaces == null ? null : p1.LikedPlaces.Select<UsersAndPlacesContext.Place, PlaceShortDto>(p => p.AdaptToShortDto()),
                Login = p1.Login,
                Email = p1.Email,
                BlackList = p1.BlackList == null ? null : p1.BlackList.Select<UsersAndPlacesContext.Place, PlaceShortDto>(p => p.AdaptToShortDto())
            };
        }
    
    }
}