using System.Linq;
using MAP.Models;
using Microsoft.EntityFrameworkCore;

namespace MAP.DbContexts
{
    public static class LINQHelpers
    {
        public static IEnumerable<UsersAndPlacesContext.Place> OrderByRatingAndTakeFromPage(
            this IEnumerable<UsersAndPlacesContext.Place> places, int page, int take)
        {
            var takedPlaces = places
                .OrderBy(p => p.LikeUserCount)
                .TakeByPage(page, take)
                .ToArray();
            var firstHalfTake = takedPlaces.Length/2;
            var secondHalftTake = takedPlaces.Length - firstHalfTake;
            var firstHalf = takedPlaces.Take(firstHalfTake)
                .OrderBy(p => (p.LikeUserCount + 1) / (p.BlackListCount + 1));
            var secondHalf = takedPlaces.Skip(firstHalfTake).Take(secondHalftTake);
            return firstHalf.Concat(secondHalf);
        }

        public static IEnumerable<UsersAndPlacesContext.Place> WhereByName(
            this IEnumerable<UsersAndPlacesContext.Place> places, string search)
        => places.Where(p => search.ToLower().Contains(p.Name) || 
                    p.Name.ToLower().Contains(search));
        
        public static IEnumerable<UsersAndPlacesContext.Place> WhereByCategory(
            this IEnumerable<UsersAndPlacesContext.Place> places, string category)
        => places.Where(p => p.Categories.FirstOrDefault(c => c.Name.ToLower() == category.ToLower()) != null);

        public static async Task<UsersAndPlacesContext.Place> FindByIdAsync(
            this IQueryable<UsersAndPlacesContext.Place> places, string placeId) =>
            await places.FirstOrDefaultAsync(p => p.Id == placeId);

        public static IEnumerable<string> SelectCategoriesByName(
            this IEnumerable<UsersAndPlacesContext.Category> categories)
        => categories.Select(c => c.Name);

        public static async Task<UsersAndPlacesContext.Category> FindByNameAsync(
            this IQueryable<UsersAndPlacesContext.Category> categories, string categoryName) =>
            await categories.FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName.ToLower());
        public static IEnumerable<TObject> TakeByPage<TObject>(
            this IEnumerable<TObject> places, int page, int take) =>
        places.Skip(take * page).Take(take);

        public static async Task<UsersAndPlacesContext.User> FindByIdAsync(
            this IQueryable<UsersAndPlacesContext.User> users, string userId)
        => await users.FirstAsync(u => u.Id == userId);

        public static IEnumerable<string> SelectById(this IEnumerable<UsersAndPlacesContext.Place> places)
        =>  places.Select(s => s.Id);

        public static IEnumerable<UsersAndPlacesContext.Place> FilterByBlackList(
            this IEnumerable<UsersAndPlacesContext.Place> places, IEnumerable<string> blackListSelectedById)
        => places.Where(p => !blackListSelectedById.Contains(p.Id));


        public static async Task<UsersAndPlacesContext.User> FindUserByLoginAndPassHash(this IQueryable<UsersAndPlacesContext.User> users,string login, string hashedPassword)
        => await users.FirstOrDefaultAsync(u => u.Login == login && u.Password == hashedPassword);

        public static UsersAndPlacesContext.Place FindById(this IEnumerable<UsersAndPlacesContext.Place> places, string placeId)
        => places.FirstOrDefault(p => p.Id == placeId);    
    }   
    public static partial class PlaceMapper
    {
        public static PlaceDto AdaptToDto(this UsersAndPlacesContext.Place p1)
        {
            return p1 == null ? null : new PlaceDto()
            {
                Id = p1.Id,
                Name = p1.Name,
                Categories = p1.Categories == null ? null : p1.Categories
                    .Select<UsersAndPlacesContext.Category, string>(funcMain1),
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
        private static string funcMain1(UsersAndPlacesContext.Category p2)
        {
            return p2 == null ? null : p2.Name;
        }
        
        
    }
}