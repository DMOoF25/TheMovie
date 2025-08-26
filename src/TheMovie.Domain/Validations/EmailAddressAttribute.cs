using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TheMovie.Domain.Validations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
internal class EmailAddressAttribute : ValidationAttribute
{
    private const string _errorMessage = "er ugyldig email adresse.";

    private static readonly Regex _emailRegex = new Regex(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override bool IsValid(object? value)
    {
        if (value == null) return true; // Optional field
        return value is string email && _emailRegex.IsMatch(email);
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} {_errorMessage}";
    }
}
