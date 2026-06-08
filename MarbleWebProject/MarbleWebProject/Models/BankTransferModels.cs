namespace MarbleWebProject.Models;

public sealed class BankTransferOrderInfoModel
{
    public int? BankAccountId { get; set; }
    public string? BankName { get; set; }
    public string? AccountHolder { get; set; }
    public string? Iban { get; set; }
    public DateTime? PaymentDueAt { get; set; }
    public string? ReceiptPath { get; set; }
    public string? ReceiptUrl { get; set; }
    public DateTime? ReceiptUploadedAt { get; set; }
    public int BankVerificationStatus { get; set; }
    public string BankVerificationStatusText { get; set; } = "";
    public string? BankVerificationReference { get; set; }
    public string? BankVerificationMessage { get; set; }
    public DateTime? BankVerificationCheckedAt { get; set; }
}

public sealed class RegisterPaymentReceiptApiRequest
{
    public string ReceiptPath { get; set; } = "";
}

public sealed class GuestPaymentReceiptApiRequest
{
    public string GuestUserId { get; set; } = "";
    public string ReceiptPath { get; set; } = "";
}
