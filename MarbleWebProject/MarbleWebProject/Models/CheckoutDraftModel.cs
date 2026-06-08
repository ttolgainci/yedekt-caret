namespace MarbleWebProject.Models;

public sealed class CheckoutDraftModel
{
    public string? GuestEmail { get; set; }
    public string? GuestFirstName { get; set; }
    public string? GuestLastName { get; set; }
    public List<CheckoutDraftAddressModel> Addresses { get; set; } = new();
    public string? SelectedShippingId { get; set; }
    public string? SelectedBillingId { get; set; }
    public bool BillingDifferent { get; set; }
    public string? PaymentMethod { get; set; }
    public int? BankAccountId { get; set; }
    public string? OrderNote { get; set; }
    public int LocalAddrSeq { get; set; } = 1;
}

public sealed class CheckoutDraftAddressModel
{
    public object? Id { get; set; }
    public string? Label { get; set; }
    public string? ContactFirstName { get; set; }
    public string? ContactLastName { get; set; }
    public string? ContactPhone { get; set; }
    public int? CountryId { get; set; }
    public string? CountryName { get; set; }
    public int? CityId { get; set; }
    public string? CityName { get; set; }
    public int? TownId { get; set; }
    public string? TownName { get; set; }
    public string? AddressLine1 { get; set; }
    public string? DisplayLine { get; set; }
    public CheckoutDraftInvoiceMetaModel? InvoiceMeta { get; set; }
}

public sealed class CheckoutDraftInvoiceMetaModel
{
    public string? InvoiceType { get; set; }
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? CompanyName { get; set; }
    public bool EInvoice { get; set; }
}
