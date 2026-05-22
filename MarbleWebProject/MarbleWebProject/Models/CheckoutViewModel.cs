namespace MarbleWebProject.Models
{
    public class CheckoutViewModel
    {
        public List<CartModel> Lines { get; set; } = new();
        public string? TotalSummaryHtml { get; set; }
        public int TotalQuantity { get; set; }
        public string? AlertMessage { get; set; }
        public bool AlertSuccess { get; set; }
        public int? LastOrderId { get; set; }
        public decimal? LastGrandTotal { get; set; }
        public string? LastCurrencyCode { get; set; }
        public bool GuestCheckoutConfigured { get; set; }
    }
}
