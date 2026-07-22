namespace MarbleWebProject.Models
{
    public class BasketReturnModel
    {
        public string TotalPrice { get; set; }
        public string DrawerTotalPrice { get; set; }
        public string SubtotalPrice { get; set; }
        public string GrossSubtotalPrice { get; set; }
        public string TaxTotalPrice { get; set; }
        public decimal? TaxTotal { get; set; }
        public int? TotalQuantity { get; set; }
        public decimal? ShippingPrice { get; set; }
        public string? CarrierName { get; set; }
        public decimal? TotalDesi { get; set; }
        public string? CurrencyName { get; set; }
    }
}
