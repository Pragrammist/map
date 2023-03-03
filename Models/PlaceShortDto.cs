namespace MAP.Models;

public class PlaceShortDto
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public decimal Lat { get; set; }

    public decimal Lon { get; set; }
}
