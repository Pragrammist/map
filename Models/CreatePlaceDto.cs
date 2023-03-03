namespace MAP.Models;

public class CreatePlaceDto
{
    public string Name { get; set; } = null!;

    public IEnumerable<string> Categories { get; set; } = Enumerable.Empty<string>(); // "Горячие достопримечательности" - сделать такую категорию

    public string Info { get; set; } = null!;

    public string Image { get; set; } = null!;

    public Object GeoJson { get; set; } = null!;

}