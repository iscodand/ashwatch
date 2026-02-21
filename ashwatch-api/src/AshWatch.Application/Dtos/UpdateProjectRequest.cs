using System.ComponentModel.DataAnnotations;

namespace AshWatch.Application.Dtos;

public class UpdateProjectRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "TenantId must be greater than zero.")]
    public int TenantId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "AuthorId must be greater than zero.")]
    public int AuthorId { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string Description { get; set; } = string.Empty;
}
