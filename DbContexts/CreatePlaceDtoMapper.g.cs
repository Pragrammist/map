using System;
using System.Collections.Generic;
using System.Linq;
using MAP.DbContexts;
using MAP.Models;

namespace MAP.DbContexts
{
    public static partial class CreatePlaceDtoMapper
    {
        public static UsersAndPlacesContext.Place AdaptToPlace(this CreatePlaceDto p1)
        {
            return p1 == null ? null : new UsersAndPlacesContext.Place()
            {
                Name = p1.Name,
                Categories = funcMain1(p1.Categories),
                Info = p1.Info,
                Image = p1.Image,
                Lat = p1.Lat,
                Lon = p1.Lon
            };
        }
        
        
        private static ICollection<UsersAndPlacesContext.Category> funcMain1(IEnumerable<string> p2)
        {
            
            
            
            
            
            return p2 is null ? null : p2.Select(c => new UsersAndPlacesContext.Category {
                Name = c
            }).ToList();
            
        }
        
        
    }
}