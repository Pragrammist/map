using System;
using System.Collections.Generic;
using System.Linq;
using MAP.DbContexts;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using MAP.Models;

namespace MAP.DbContexts
{
    public static partial class CreatePlaceDtoMapper
    {
        public static async Task<UsersAndPlacesContext.Place> AdaptToPlace(this CreatePlaceDto p1, UsersAndPlacesContext context)
        {
            return p1 == null ? null : new UsersAndPlacesContext.Place()
            {
                Name = p1.Name,
                Categories = await Categories(p1.Categories, context),
                Info = p1.Info,
                Image = p1.Image,
                GeoJson = JsonSerializer.Serialize(p1.GeoJson)
            };
        }
        
        private static async Task<ICollection<UsersAndPlacesContext.Category>> Categories(IEnumerable<string> categories, UsersAndPlacesContext context)
        {
            var categoriesResult = new List<UsersAndPlacesContext.Category>();
            foreach(var categoryName in categories)
            {
                var category = await GetOrCreateCategory(categoryName, context);
                categoriesResult.Add(category);
            }
            return categoriesResult;
            
        }
        
        static async Task<UsersAndPlacesContext.Category> GetOrCreateCategory(string categoryName, UsersAndPlacesContext context)
        {
            var category = await context.Categories.FindByNameAsync(categoryName);
            if(category is null)
                category = new UsersAndPlacesContext.Category { Name = categoryName };
            return category;
        }
    }
}