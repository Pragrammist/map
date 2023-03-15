using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MAP.Controllers;

[ApiController]
[Route("placeimage")]
public class ImageController : ControllerBase
{
    const string PLACE_IMAGES_DIR = "PlaceImages";
    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if(!file.ContentType.Contains("image"))
            throw new InvalidOperationException("file not valid format");
        
        var ext = Path.GetExtension(file.ContentType.Replace('/', '.'));
        var rFileName = Path.GetRandomFileName().Replace(".", string.Empty);
        var pathToFolder = Directory.CreateDirectory(
            Path.Combine(Directory.GetCurrentDirectory(), 
            PLACE_IMAGES_DIR)
        );

        var imageName = $"{rFileName}{ext}";
        var pathToImg = Path.Combine(pathToFolder.Name, imageName);
        using(var fileStream = new FileStream(pathToImg, FileMode.Create))
            await file.CopyToAsync(fileStream);
        return Content(imageName);
    }
    
    [HttpDelete("{name}")]
    [Authorize(Roles = "admin")]
    public IActionResult Delete(string name)
    {
        var pathToFolder = Directory.CreateDirectory(
            Path.Combine(Directory.GetCurrentDirectory(), 
            PLACE_IMAGES_DIR)
        );   
        var pathToImg = Path.Combine(pathToFolder.Name, name);
        System.IO.File.Delete(pathToImg);
        return Ok();
    }
}