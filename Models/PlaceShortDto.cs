namespace MAP.Models;

public class PlaceShortDto
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string GeoJson { get; set; } = null!;

    public int UserCount { get; set; }
}
