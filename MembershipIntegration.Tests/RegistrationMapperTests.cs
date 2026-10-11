using MembershipIntegration.Models;
using MembershipIntegration.Services;
using Xunit;

namespace MembershipIntegration.Tests;

public class RegistrationMapperTests
{
    private readonly RegistrationMapper _mapper = new();

    private static CrmRegistration ValidRegistration() => new()
    {
        FirstName = "Alex",
        LastName = "Nguyen",
        DateOfBirth = "1990-04-12",
        Email = "alex.nguyen@example.com",
        MembershipType = "Single",
        RegisteredAt = "2026-06-01T09:00:00Z"
    };

    [Fact]
    public void Map_ValidRegistration_ReturnsCorrectLegacyPayload()
    {
        var input = ValidRegistration();

        var (validation, payload) = _mapper.Map(input);

        Assert.True(validation.IsValid);
        Assert.NotNull(payload);
        Assert.Equal("Alex", payload!.Member.GivenName);
        Assert.Equal("Nguyen", payload.Member.FamilyName);
        Assert.Equal("12/04/1990", payload.Member.Dob);
        Assert.Equal("alex.nguyen@example.com", payload.Member.Contact.Email);
        Assert.Equal("S", payload.Member.PlanCode);
        Assert.Equal("MILKYWAY", payload.Member.Source);
    }

    [Theory]
    [InlineData("Single", "S")]
    [InlineData("Family", "F")]
    [InlineData("Couple", "C")]
    [InlineData("single", "S")] // case-insensitive
    [InlineData("FAMILY", "F")]
    public void Map_PlanCodeMapping_IsCorrect(string membershipType, string expectedPlanCode)
    {
        var input = ValidRegistration();
        input.MembershipType = membershipType;

        var (validation, payload) = _mapper.Map(input);

        Assert.True(validation.IsValid);
        Assert.NotNull(payload);
        Assert.Equal(expectedPlanCode, payload!.Member.PlanCode);
    }

    [Theory]
    [InlineData("1990-04-12", "12/04/1990")]
    [InlineData("12/04/1990", "12/04/1990")]
    [InlineData("1/5/1985", "01/05/1985")]
    public void Map_DateOfBirth_FormatsToDdMmYyyy(string inputDob, string expectedDob)
    {
        var input = ValidRegistration();
        input.DateOfBirth = inputDob;

        var (validation, payload) = _mapper.Map(input);

        Assert.True(validation.IsValid);
        Assert.NotNull(payload);
        Assert.Equal(expectedDob, payload!.Member.Dob);
    }

    [Fact]
    public void Map_IgnoresExtraFields()
    {
        // Simulate extra fields by using a richer object; System.Text.Json ignores unknown properties by default.
        var json = """
        {
            "firstName": "Alex",
            "lastName": "Nguyen",
            "dateOfBirth": "1990-04-12",
            "email": "alex.nguyen@example.com",
            "membershipType": "Single",
            "registeredAt": "2026-06-01T09:00:00Z",
            "extraField": "should-be-ignored",
            "anotherExtra": 123
        }
        """;

        var registration = System.Text.Json.JsonSerializer.Deserialize<CrmRegistration>(
            json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var (validation, payload) = _mapper.Map(registration);

        Assert.True(validation.IsValid);
        Assert.NotNull(payload);
        Assert.Equal("Alex", payload!.Member.GivenName);
    }

    [Fact]
    public void Validate_MissingFirstName_ReturnsError()
    {
        var input = ValidRegistration();
        input.FirstName = null;

        var result = _mapper.Validate(input);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("firstName", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_EmptyLastName_ReturnsError()
    {
        var input = ValidRegistration();
        input.LastName = "   ";

        var result = _mapper.Validate(input);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("lastName", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_MissingDateOfBirth_ReturnsError()
    {
        var input = ValidRegistration();
        input.DateOfBirth = "";

        var result = _mapper.Validate(input);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("dateOfBirth", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_InvalidDateOfBirth_ReturnsError()
    {
        var input = ValidRegistration();
        input.DateOfBirth = "not-a-date";

        var result = _mapper.Validate(input);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("dateOfBirth", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_MissingEmail_ReturnsError()
    {
        var input = ValidRegistration();
        input.Email = null;

        var result = _mapper.Validate(input);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("email", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-at-sign.com")]
    [InlineData("@nodomain.com")]
    [InlineData("spaces emma@example.com")]
    public void Validate_InvalidEmail_ReturnsError(string email)
    {
        var input = ValidRegistration();
        input.Email = email;

        var result = _mapper.Validate(input);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("email", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_MissingMembershipType_ReturnsError()
    {
        var input = ValidRegistration();
        input.MembershipType = null;

        var result = _mapper.Validate(input);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("membershipType", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("Premium")]
    [InlineData("Gold")]
    [InlineData("")]
    public void Validate_UnknownMembershipType_ReturnsError(string membershipType)
    {
        var input = ValidRegistration();
        input.MembershipType = membershipType;

        var result = _mapper.Validate(input);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("membershipType", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_NullRegistration_ReturnsError()
    {
        var result = _mapper.Validate(null);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Map_MultipleValidationErrors_ReturnsAll()
    {
        var input = new CrmRegistration
        {
            FirstName = "",
            LastName = "",
            DateOfBirth = "bad",
            Email = "bad",
            MembershipType = "Unknown"
        };

        var (validation, payload) = _mapper.Map(input);

        Assert.False(validation.IsValid);
        Assert.Null(payload);
        Assert.True(validation.Errors.Count >= 4);
    }

    [Fact]
    public void Map_TrimsWhitespaceFromNamesAndEmail()
    {
        var input = ValidRegistration();
        input.FirstName = "  Alex  ";
        input.LastName = "  Nguyen  ";
        input.Email = "  alex.nguyen@example.com  ";

        var (validation, payload) = _mapper.Map(input);

        Assert.True(validation.IsValid);
        Assert.Equal("Alex", payload!.Member.GivenName);
        Assert.Equal("Nguyen", payload.Member.FamilyName);
        Assert.Equal("alex.nguyen@example.com", payload.Member.Contact.Email);
    }
}
