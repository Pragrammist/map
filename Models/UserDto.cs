namespace MAP.Models;

public class UserDto 
{
    public string Id { get; set; } = null!;
    
    public IEnumerable<string> Places { get; set; } = Enumerable.Empty<string>();

    public string Name { get; set; } = null!;

    public string? Email { get; set; }
}
