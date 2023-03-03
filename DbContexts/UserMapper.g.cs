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
                Places = p1.Places == null ? null : p1.Places.Select<UsersAndPlacesContext.Place, PlaceShortDto>(p => p.AdaptToShortDto()),
                Login = p1.Login,
                Email = p1.Email
            };
        }
        

    }
}