namespace MarbleWebProject.Models;

public sealed class CustomerInvoiceListItemModel
{
    public int ID { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public int? ShopOrderId { get; set; }
    public string? OrderNumber { get; set; }
    public int Status { get; set; }
    public string StatusText { get; set; } = "";
    public decimal GrandTotal { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public DateTime? IssueDate { get; set; }
    public bool HasPdf { get; set; }
    public string? PdfUrl { get; set; }
}

public sealed class CustomerInvoiceDetailModel
{
    public int ID { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public int? ShopOrderId { get; set; }
    public string? OrderNumber { get; set; }
    public int Status { get; set; }
    public string StatusText { get; set; } = "";
    public decimal SubTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public DateTime? IssueDate { get; set; }
    public string? PdfUrl { get; set; }
    public List<CustomerInvoiceLineModel> Items { get; set; } = new();
}

public sealed class CustomerInvoiceLineModel
{
    public int LineNo { get; set; }
    public string ProductName { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
