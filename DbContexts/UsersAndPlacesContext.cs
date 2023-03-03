using System.ComponentModel.DataAnnotations.Schema;
using MAP.Models;

using Microsoft.EntityFrameworkCore;

namespace MAP.DbContexts;

public class UsersAndPlacesContext : DbContext
{
    public UsersAndPlacesContext()
    {
        base.Database.Migrate();
    }
    

    
    public class User
    {
        public string Id { get; set; } = null!;
    
        public ICollection<Place>? Places { get; set; } 

        public string Name { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string? Email { get; set; }
    }

    public class Category
    {
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;
    }

    
    public class Place
    {
        public string Id { get; set; } = null!;
    
        public string Name { get; set; } = null!;

        public ICollection<User>? Users { get; set; } 

        public ICollection<Category> Categories { get; set; } = Enumerable.Empty<Category>().ToList(); // "Горячие достопримечательности" - сделать такую категорию

        public string Info { get; set; } = null!;

        public string Image { get; set; } = null!;

        public decimal Lat { get; set; }

        public decimal Lon { get; set; }
    }
    
    public DbSet<User> Users { get; set; } = null!;
    
    public DbSet<Place> Places { get; set; } = null!;

    public DbSet<Category> Categories { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source=map.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
    }

}

