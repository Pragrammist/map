using System.Reflection;
using MAP.DbContexts;
using MAP.Services;
using Mapster;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
const string PLACE_IMAGES_DIR = "PlaceImages";
Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), PLACE_IMAGES_DIR));
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer()
                .AddSwaggerGen(options => {
                    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    options.IncludeXmlComments(System.IO.Path.Combine(AppContext.BaseDirectory, xmlFilename), includeControllerXmlComments: true);
                });;
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<UsersAndPlacesContext>(options => { options.UseSqlite($"Data Source=map.db"); });
builder.Services.AddTransient<PasswordHasher, PasswordHasherImpl>();

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
