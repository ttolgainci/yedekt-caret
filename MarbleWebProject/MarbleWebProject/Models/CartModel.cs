namespace MarbleWebProject.Models
{
    public class CartModel
    {
        public int ProductID { get; set; }
        public int? ProductVariantID { get; set; }
        public string ProductName { get; set; }
        public string MainImage { get; set; }
        public string Url { get; set; }
        public decimal? Price { get; set; }
        public string CurrencyName { get; set; }
        public int? CartQuantity { get; set; }
    }
   public class CartFullModel
    {
        public List<CartModel> CartList = new List<CartModel>();
        public CartInfoModel Info = new CartInfoModel();
    }
    public class CartInfoModel
    {
        public string Total { get; set; }
        public int? TotalQuantity { get; set; }

    }
    public class BasketSetModel
    {
        public List<CartModel> CartList = new List<CartModel>();
        public CartInfoModel Info = new CartInfoModel();
    }
}
