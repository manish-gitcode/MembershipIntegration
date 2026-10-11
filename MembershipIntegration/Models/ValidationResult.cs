namespace MembershipIntegration.Models;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ValidationResult Ok() => new() { IsValid = true };

    public static ValidationResult Fail(params string[] errors) =>
        new() { IsValid = false, Errors = errors.ToList() };
}
