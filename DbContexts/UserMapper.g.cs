using System.Linq;
using MAP.Models;

namespace MAP.DbContexts
{
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