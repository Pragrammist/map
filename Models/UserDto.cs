using System.Collections.Generic;
using System.Linq;

namespace MAP.Models;

public class UserDto 
{
    public string Id { get; set; } = null!;
    
    public IEnumerable<PlaceShortDto>? LikedPlaces { get; set; } 

    public IEnumerable<PlaceShortDto>? BlackList { get; set; } 

    public string Login { get; set; } = null!;

    public string? Email { get; set; }
}
