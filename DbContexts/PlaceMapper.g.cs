using System.Linq;
using MAP.Models;

namespace MAP.DbContexts
{
    public static partial class PlaceMapper
    {
        public static PlaceDto AdaptToDto(this UsersAndPlacesContext.Place p1)
        {
            return p1 == null ? null : new PlaceDto()
            {
                Id = p1.Id,
                Name = p1.Name,
                Categories = p1.Categories == null ? null : p1.Categories.Select<UsersAndPlacesContext.Category, string>(funcMain1),
                Info = p1.Info,
                Image = p1.Image,
                GeoJson = p1.GeoJson,
                UserCount = p1.UserCount
            };
        }
        
        public static PlaceShortDto AdaptToShortDto(this UsersAndPlacesContext.Place p1)
        {
            return p1 == null ? null : new PlaceShortDto()
            {
                Id = p1.Id,
                Name = p1.Name,
                GeoJson = p1.GeoJson,
                UserCount = p1.UserCount
            };
        }
        
        private static string funcMain1(UsersAndPlacesContext.Category p2)
        {
            return p2 == null ? null : p2.ToString();
        }
        
        
    }
}