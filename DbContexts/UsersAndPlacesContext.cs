using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using MAP.Models;
using Mapster;
using System;

using Microsoft.EntityFrameworkCore;

namespace MAP.DbContexts;

public class UsersAndPlacesContext : DbContext
{
    public UsersAndPlacesContext(DbContextOptions<UsersAndPlacesContext> options)
            : base(options)
    {
        
        base.Database.EnsureCreated();
    }
    

    [Table("users")]
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = null!;
    
        public ICollection<Place>? Places { get; set; } 

        public string Login { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string? Email { get; set; }
    }

    [Table("categories")]
    public class Category
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;
    }

   
    [Table("places")]
    public class Place
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = null!;
    
        public string Name { get; set; } = null!;

        public ICollection<User>? Users { get; set; } 

        public ICollection<Category> Categories { get; set; } = Enumerable.Empty<Category>().ToList(); // "Горячие достопримечательности" - сделать такую категорию

        public string Info { get; set; } = null!;

        public string Image { get; set; } = null!;

        public string GeoJson { get; set; } = null!;
    }
    
    public DbSet<User> Users { get; set; } = null!;
    
    public DbSet<Place> Places { get; set; } = null!;

    public DbSet<Category> Categories { get; set; } = null!;

    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(p => p.Login).IsUnique();
        modelBuilder.Entity<User>().HasIndex(p => p.Email).IsUnique();

        base.OnModelCreating(modelBuilder);
        
    }

}

