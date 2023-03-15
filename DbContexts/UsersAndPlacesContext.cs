using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using MAP.Models;
using Mapster;
using System;

using Microsoft.EntityFrameworkCore;

namespace MAP.DbContexts;
//Это класс нужен, чтобы код связать с бд
//Он из Entity Framework(ORM)
// https://learn.microsoft.com/ru-ru/ef/core/
public class UsersAndPlacesContext : DbContext
{
    public UsersAndPlacesContext(DbContextOptions<UsersAndPlacesContext> options)
            : base(options)
    {
        
        base.Database.EnsureCreated(); //создает бд или подключает существующуб
    }
    // модели которые используются здесь не относятся к бизнеслогике, и нужны для работы ORM
    // соответственно если проект будет переписывать в более адектввтную архитектур
    // например по "Чистой архитектура", то эти классы сугубо относятся к деталям реализации самой бд
    // ОНИ НЕ ЯВЛЯЮТСЯ ЧАСТЬЮ БИЗНЕС ЛОГИКИ
    // Но т.к в проекте нет репозиториев и пр. что отделяет ее от всего остального
    // Эти классы используются в контроллерх
    // Это нужно для более простой разрабоки в начале
    // если этот код планируется расширить и использовать в комерческих целях
    // то нужно из контроллеров убрать ссылку на эти классы 
    // отделить бизнес логику, веб и инфраструктуру(бд, шифрование, микросервисность и пр)
    // 

    // Если нужно понимание что для чего нужно, то вот документация по фреймворку
    // https://learn.microsoft.com/ru-ru/ef/core/
    public class UserAndPlaceBlackList
    {
        public string UserId { get; set; } = null!;

        public string PlaceId { get; set; } = null!;
    }

    [Table("users")]
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = null!;
    
        public ICollection<Place> LikedPlaces { get; set; } = new List<Place>();

        public string Login { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string? Email { get; set; }

        public ICollection<Place> BlackList { get; set; } = new List<Place>();
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

        public ICollection<User> LikedUsers { get; set; } = null!;

        public ICollection<User> BlacklistUsers { get; set; }  = null!;

        public ICollection<Category> Categories { get; set; } = Enumerable.Empty<Category>().ToList(); // "Горячие достопримечательности" - сделать такую категорию

        public string Info { get; set; } = null!;

        public int LikeUserCount { get; set; }

        public string Image { get; set; } = null!;

        public string GeoJson { get; set; } = null!;

        public int BlackListCount { get; set; }
    }
    
    public DbSet<User> Users { get; set; } = null!;
    
    public DbSet<Place> Places { get; set; } = null!;

    public DbSet<Category> Categories { get; set; } = null!;

    
    // https://learn.microsoft.com/ru-ru/ef/core/
    // 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(p => p.Login).IsUnique();
        modelBuilder.Entity<Category>().HasIndex(p => p.Name).IsUnique();
        modelBuilder.Entity<User>().HasIndex(p => p.Email).IsUnique();
        modelBuilder.Entity<User>().HasMany(u => u.BlackList).WithMany(p => p.BlacklistUsers);
        modelBuilder.Entity<User>().HasMany(u => u.LikedPlaces).WithMany(p => p.LikedUsers);
        base.OnModelCreating(modelBuilder);
        
    }

}

