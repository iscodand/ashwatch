using System.ComponentModel.DataAnnotations;

namespace AshWatch.Application.Dtos;

public class CreateTenantRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "AuthorId must be greater than zero.")]
    public int AuthorId { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string Description { get; set; } = string.Empty;
}
