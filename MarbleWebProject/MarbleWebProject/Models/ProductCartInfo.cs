namespace MarbleWebProject.Models
{
	public class ProductCartInfo
	{
		public string ProductName { get; set; }
		public decimal? Price { get; set; }
		public string CurrencyName { get; set; }
		public string ProductModel { get; set; }
		public string ProductCode { get; set; }
		public int? MinimumQuantity { get; set; }
		public int? Quantity { get; set; }
		public int ProductID { get; set; }
		public string MainImage { get; set; }
	}
}
