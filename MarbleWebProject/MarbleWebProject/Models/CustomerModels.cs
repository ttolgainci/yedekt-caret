namespace MarbleWebProject.Models;

public sealed class CustomerRegisterForm
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? CustomerType { get; set; }
    public string? CompanyName { get; set; }
    public string? TaxOffice { get; set; }
    public string? TaxNumber { get; set; }
    public bool AcceptKvkk { get; set; } = true;
    public bool MarketingConsent { get; set; }
}

public sealed class CustomerLoginForm
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public sealed class CustomerProfileModel
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string CustomerType { get; set; } = "Retail";
    public bool IsActive { get; set; }
    public string? CompanyName { get; set; }
    public string? TaxOffice { get; set; }
    public string? TaxNumber { get; set; }
    public DateTime? BirthDate { get; set; }
}

public sealed class CustomerAuthResult
{
    public string Token { get; set; } = "";
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public string? LanguageCode { get; set; }
    public string? LanguageName { get; set; }
    public string? LanguageCulture { get; set; }
    public CustomerProfileModel Customer { get; set; } = new();
}
