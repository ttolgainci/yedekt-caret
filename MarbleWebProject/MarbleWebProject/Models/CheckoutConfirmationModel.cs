namespace MarbleWebProject.Models;

public sealed class CheckoutConfirmationModel
{
    public int OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public decimal GrandTotal { get; set; }
    public string CurrencyCode { get; set; } = "";
    public string CurrencySymbol { get; set; } = "";
    public string PaymentMethod { get; set; } = "CashOnDelivery";
    public string? CustomerEmail { get; set; }
    public DateTime? PaymentDueAt { get; set; }
    public int? BankAccountId { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountHolder { get; set; }
    public string? BankIban { get; set; }
    public string? BankInstructions { get; set; }
}
