using System.ComponentModel.DataAnnotations;

namespace Bridge.Api.Dtos.Engineers;

public class UpdateEngineerRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Bio { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string AvoidedWorkNote { get; set; } = string.Empty;
}