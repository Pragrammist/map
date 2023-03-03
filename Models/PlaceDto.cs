using System.Collections.Generic;
using System.Linq;
using Mapster;

namespace MAP.Models;

public class PlaceDto
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public IEnumerable<string> Categories { get; set; } = Enumerable.Empty<string>(); // "Горячие достопримечательности" - сделать такую категорию

    public string Info { get; set; } = null!;

    public string Image { get; set; } = null!;

    public string GeoJson { get; set; } = null!;

    public int UserCount { get; set; }

}
