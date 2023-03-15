using System.Linq;
using MAP.Models;
using Microsoft.EntityFrameworkCore;

namespace MAP.DbContexts;

public static class LINQHelpers
    {

        //здесь контринтуитивная сортировка
        //сначала берутся самые популярные места
        //по тому сколько раз место добавили в любимое
        //потом вычисляется нужная страница(page, take)
        //далее то что получили делится на два подмассива
        //первая половина
        //вторая половина
        //первая половина сортируется по формуле
        //которая помогает вычислить насколько рейтинг надежный 
        //(p.LikeUserCount + 1) / (p.BlackListCount + 1) вот это строка
        //со второй половиной ничего не делается
        // далее две эти половины объединяются и возвращаются
        public static IEnumerable<UsersAndPlacesContext.Place> OrderByRatingAndTakeFromPage(
            this IEnumerable<UsersAndPlacesContext.Place> places, int page, int take)
        {
            var takedPlaces = places
                .OrderBy(p => p.LikeUserCount)
                .TakeByPage(page, take)
                .ToArray(); // сортировка по поулярности и берется нужная страница
            
            // сколько нужно взять для первой половины
            var firstHalfTake = takedPlaces.Length/2;
            //сколько нужно взять для второй
            var secondHalftTake = takedPlaces.Length - firstHalfTake;
            // берется первая половина
            var firstHalf = takedPlaces.Take(firstHalfTake)
            //сортируется по той самой формуле
                .OrderBy(p => (p.LikeUserCount + 1) / (p.BlackListCount + 1));
            // берертся втарвая половина
            var secondHalf = takedPlaces.Skip(firstHalfTake).Take(secondHalftTake);
            return firstHalf.Concat(secondHalf);//объединение
        }

        public static IEnumerable<UsersAndPlacesContext.Place> WhereByName(
            this IEnumerable<UsersAndPlacesContext.Place> places, string search)
        => places.Where(p => search.ToLower().Contains(p.Name) || 
                    p.Name.ToLower().Contains(search));
        
        public static IEnumerable<UsersAndPlacesContext.Place> WhereByCategory(
            this IEnumerable<UsersAndPlacesContext.Place> places, string category)
        => places.Where(p => p.Categories.FirstOrDefault(c => c.Name.ToLower() == category.ToLower()) != null);

        public static async Task<UsersAndPlacesContext.Place?> FindByIdAsync(
            this IQueryable<UsersAndPlacesContext.Place> places, string placeId) =>
            await places.FirstOrDefaultAsync(p => p.Id == placeId);

        public static IEnumerable<string> SelectCategoriesByName(
            this IEnumerable<UsersAndPlacesContext.Category> categories)
        => categories.Select(c => c.Name);

        public static async Task<UsersAndPlacesContext.Category?> FindByNameAsync(
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


        public static async Task<UsersAndPlacesContext.User?> FindUserByLoginAndPassHash(this IQueryable<UsersAndPlacesContext.User> users,string login, string hashedPassword)
        => await users.FirstOrDefaultAsync(u => u.Login == login && u.Password == hashedPassword);

        public static UsersAndPlacesContext.Place? FindById(this IEnumerable<UsersAndPlacesContext.Place> places, string placeId)
        => places.FirstOrDefault(p => p.Id == placeId);    
    }