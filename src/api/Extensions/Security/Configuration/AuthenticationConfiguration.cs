using System.ComponentModel.DataAnnotations;

namespace ProjectEstimate.Extensions.Security.Configuration;

internal class AuthenticationConfiguration
{
    [Required]
    public required string Authority { get; set; }

    [Required]
    public required string Audience { get; set; }
}
