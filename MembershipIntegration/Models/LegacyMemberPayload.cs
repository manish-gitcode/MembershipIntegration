using System.Text.Json.Serialization;

namespace MembershipIntegration.Models;

public class LegacyMemberPayload
{
    [JsonPropertyName("member")]
    public LegacyMember Member { get; set; } = new();
}

public class LegacyMember
{
    [JsonPropertyName("given_name")]
    public string GivenName { get; set; } = string.Empty;

    [JsonPropertyName("family_name")]
    public string FamilyName { get; set; } = string.Empty;

    [JsonPropertyName("dob")]
    public string Dob { get; set; } = string.Empty;

    [JsonPropertyName("contact")]
    public LegacyContact Contact { get; set; } = new();

    [JsonPropertyName("plan_code")]
    public string PlanCode { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = "MILKYWAY";
}

public class LegacyContact
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}
