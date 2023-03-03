namespace MAP.Models;

public class UserDto 
{
    public string Id { get; set; } = null!;
    
    public IEnumerable<PlaceShortDto> Places { get; set; } = Enumerable.Empty<PlaceShortDto>();

    public string Name { get; set; } = null!;

    public string? Email { get; set; }
}
