using System.Text.Json.Serialization;

namespace MembershipIntegration.Models;

public class CrmRegistration
{
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("dateOfBirth")]
    public string? DateOfBirth { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("membershipType")]
    public string? MembershipType { get; set; }

    [JsonPropertyName("registeredAt")]
    public string? RegisteredAt { get; set; }
}
