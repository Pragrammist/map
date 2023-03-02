namespace MAP.Models.Entities;

public class User 
{
    public string Id { get; set; } = null!;
    
    public IEnumerable<string> Places { get; set; } = Enumerable.Empty<string>();

    public string Name { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Email { get; set; }
}
