namespace AshWatch.Domain.Entities;

public class Project
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int AuthorId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
