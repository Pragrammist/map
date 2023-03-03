using MAP.DbContexts;
using Mapster;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
const string PLACE_IMAGES_DIR = "PlaceImages";
Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), PLACE_IMAGES_DIR));
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<UsersAndPlacesContext>(options => { options.UseSqlite($"Data Source=map.db"); });

var app = builder.Build();
app.UseStaticFiles(new StaticFileOptions
    {
        
        FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), PLACE_IMAGES_DIR)),
        RequestPath = "/placeimages"
    });
// Configure the HTTP request pipeline.
app.UseSwagger();
    app.UseSwaggerUI(opt => {
        opt.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        opt.RoutePrefix = string.Empty;
    });




app.UseAuthentication();    // аутентификация
app.UseAuthorization();     // авторизация

app.MapControllers();

app.Run();
