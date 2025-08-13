using System.ComponentModel.DataAnnotations;

namespace ProjectEstimate.Extensions.Security.Configuration;

internal class SecurityConfiguration
{
    public const string SectionName = "Security";

    [Required]
    public required AuthenticationConfiguration Authentication { get; set; }
}
