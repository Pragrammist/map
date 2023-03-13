using System.Linq;
using MAP.Models;

namespace MAP.DbContexts
{
    public static class LINQHelpers
    {
        public static IEnumerable<UsersAndPlacesContext.Place> OrderByRatingAndTakeFromPage(this IEnumerable<UsersAndPlacesContext.Place> places, int page, int take)
        {
            var takedPlaces = places.Skip(page * take).Take(take).OrderBy(p => p.LikeUserCount).ToArray();
            var firstHalfTake = takedPlaces.Length/2;
            var secondHalftTake = takedPlaces.Length - firstHalfTake;
            var firstHalf = takedPlaces.Take(firstHalfTake).OrderBy(p => (p.LikeUserCount + 1) / (p.BlackListCount + 1));
            var secondHalf = takedPlaces.Skip(firstHalfTake).Take(secondHalftTake);
            return firstHalf.Concat(secondHalf);
        }
    }
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
        
        private static string funcMain1(UsersAndPlacesContext.Category p2)
        {
            return p2 == null ? null : p2.Name;
        }
        
        
    }
}