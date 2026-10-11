using System.Globalization;
using System.Text.RegularExpressions;
using MembershipIntegration.Models;

namespace MembershipIntegration.Services;

public class RegistrationMapper
{
    private static readonly Dictionary<string, string> PlanCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Single"] = "S",
        ["Family"] = "F",
        ["Couple"] = "C"
    };

    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);

    public (ValidationResult Validation, LegacyMemberPayload? Payload) Map(CrmRegistration? input)
    {
        var validation = Validate(input);
        if (!validation.IsValid)
            return (validation, null);

        var payload = new LegacyMemberPayload
        {
            Member = new LegacyMember
            {
                GivenName = input!.FirstName!.Trim(),
                FamilyName = input.LastName!.Trim(),
                Dob = ToLegacyDate(input.DateOfBirth!),
                Contact = new LegacyContact { Email = input.Email!.Trim() },
                PlanCode = PlanCodes[input.MembershipType!],
                Source = "MILKYWAY"
            }
        };

        return (validation, payload);
    }

    public ValidationResult Validate(CrmRegistration? input)
    {
        if (input == null)
            return ValidationResult.Fail("Registration payload is required.");

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(input.FirstName))
            errors.Add("firstName is required and cannot be empty.");

        if (string.IsNullOrWhiteSpace(input.LastName))
            errors.Add("lastName is required and cannot be empty.");

        if (string.IsNullOrWhiteSpace(input.DateOfBirth))
            errors.Add("dateOfBirth is required and cannot be empty.");
        else if (!TryParseDob(input.DateOfBirth, out _))
            errors.Add("dateOfBirth must be a valid date (expected formats: yyyy-MM-dd or dd/MM/yyyy).");

        if (string.IsNullOrWhiteSpace(input.Email))
            errors.Add("email is required and cannot be empty.");
        else if (!EmailPattern.IsMatch(input.Email.Trim()))
            errors.Add("email is not a plausible email address.");

        if (string.IsNullOrWhiteSpace(input.MembershipType))
            errors.Add("membershipType is required and cannot be empty.");
        else if (!PlanCodes.ContainsKey(input.MembershipType.Trim()))
            errors.Add("membershipType must be one of: Single, Family, Couple.");

        return errors.Count == 0 ? ValidationResult.Ok() : ValidationResult.Fail(errors.ToArray());
    }

    private static bool TryParseDob(string value, out DateTime date)
    {
        string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "yyyy/MM/dd", "MM/dd/yyyy" };
        return DateTime.TryParseExact(value.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    private static string ToLegacyDate(string value)
    {
        TryParseDob(value, out var date);
        return date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
    }
}
