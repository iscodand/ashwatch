using System.ComponentModel.DataAnnotations;

namespace AshWatch.Application.Dtos;

public class CreateProjectRequest
{
    public Guid TenantId { get; set; }

    public Guid AuthorId { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string Description { get; set; } = string.Empty;
}
