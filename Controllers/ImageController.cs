using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MAP.Controllers;

[ApiController]
[Route("placeimage")]
public class ImageController : ControllerBase
{
    const string PLACE_IMAGES_DIR = "PlaceImages";

    /// <summary>
    /// загрузка файл на сервер.
    /// </summary>
    /// <remarks>
    /// Если нужно показать файл то он находится по пути
    /// localhost/PlaceImages/название_файла_которое_возвращает_сервер.jpg
    /// отправлять фото через форму
    /// </remarks>
    /// <param name="file">файл виде формы</param>
    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        // обычно при отправке фото
        // ContentType такого формата image/формат_изображения
        // чтобы можно было хранить разные форматы изображения
        // проверяется лишь наличие подстроки image
        
        if(!file.ContentType.Contains("image"))
            throw new InvalidOperationException("file not valid format");
        
        // GetExtension возвращает расширения файла
        // здесь, к примеру если ContentType будет image/jpg
        // сначала будет преобразован в image.jpg(Метод Replace)
        // GetExtension по итогу даст .jpg
        var ext = Path.GetExtension(file.ContentType.Replace('/', '.'));
        
        // GetRandomFileName может давать названия с точкой
        // Replace(".", string.Empty) убирает точку
        // подменяя ее пустотой
        var rFileName = Path.GetRandomFileName().Replace(".", string.Empty);
        var pathToFolder = Directory.CreateDirectory(
            Path.Combine(
                Directory.GetCurrentDirectory(), 
                PLACE_IMAGES_DIR
            )
        );

        var imageName = $"{rFileName}{ext}"; // точка уже есть в переменой ext
        var pathToImg = Path.Combine(pathToFolder.Name, imageName);
        using(var fileStream = new FileStream(pathToImg, FileMode.Create))
            await file.CopyToAsync(fileStream);
        //возвращается сгенерированное имя
        return Content(imageName);
    }
    
    /// <summary>
    /// удаляет файл
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="name">названи файла</param>
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