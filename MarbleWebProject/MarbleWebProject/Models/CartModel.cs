namespace MarbleWebProject.Models
{
    public class CartModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string MainImage { get; set; }
        public string Url { get; set; }
        public decimal? Price { get; set; }
        public string CurrencyName { get; set; }
        public int? CartQuantity { get; set; }
        public int? TaxPercent { get; set; }
    }
   public class CartFullModel
    {
        public List<CartModel> CartList = new List<CartModel>();
        public CartInfoModel Info = new CartInfoModel();
    }
    public class CartInfoModel
    {
        public string Total { get; set; }
        public string Subtotal { get; set; }
        public string GrossSubtotal { get; set; }
        public decimal? GrossSubtotalValue { get; set; }
        public string TaxTotal { get; set; }
        public decimal? TaxTotalValue { get; set; }
        public string GrandTotal { get; set; }
        public int? TotalQuantity { get; set; }
        public decimal? ShippingPrice { get; set; }
        public string? CarrierName { get; set; }
        public decimal? TotalDesi { get; set; }
        public int? CarrierId { get; set; }
        public string CurrencyName { get; set; } = string.Empty;
    }
    public class BasketSetModel
    {
        public List<CartModel> CartList = new List<CartModel>();
        public CartInfoModel Info = new CartInfoModel();
    }
}
