namespace JJBussert.Aspire.Domain;

public class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required string Role { get; set; } // "Admin" or "Basic"
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
