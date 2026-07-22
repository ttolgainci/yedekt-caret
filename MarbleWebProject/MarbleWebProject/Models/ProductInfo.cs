using System.ComponentModel.DataAnnotations.Schema;



namespace MarbleWebProject.Models

{

    public class ProductInfo

    {

		public int ProductID { get; set; }

		public int CategoryID { get; set; }

		public string ProductName { get; set; }

        public string? Description { get; set; }

        public string? ShortDescription { get; set; }

        public string? Tags { get; set; }

        public string? MetaTitle { get; set; }

        public string? MetaKeyword { get; set; }

        public string? MetaDescription { get; set; }

        public decimal? Price { get; set; }

        public decimal? ShippingPrice { get; set; }

        public string CurrencyName { get; set; }

        public string CurrencyCode { get; set; }

        public string ProductModel { get; set; }

        public string ProductCode { get; set; }

        public string Sku { get; set; } = string.Empty;

        public int? MinimumQuantity { get; set; }

        public int? Quantity { get; set; }

        public string StockStatus { get; set; }

        public int? StockStatusID { get; set; }

        public string StockStatusColor { get; set; } = string.Empty;

        public int? BrandID { get; set; }

        public string BrandName { get; set; } = string.Empty;

        public string BrandPicture { get; set; } = string.Empty;

        public string? BrandSlug { get; set; }

        public List<string> OemCodes { get; set; } = new();

        public string Url { get; set; }

        public string MainImage { get; set; }

        public DateTime? DateAvailable { get; set; }

        public int? CartQuantity { get; set; }

        public bool HasBasket { get; set; }

        public int? BasketQuantity { get; set; }

        [Column(TypeName = "decimal(15,8)")]

        public decimal? Weight { get; set; }

        public string WeightClassUnit { get; set; }

        [Column(TypeName = "decimal(15,8)")]

        public decimal? Length { get; set; }

        [Column(TypeName = "decimal(15,8)")]

        public decimal? Width { get; set; }

        [Column(TypeName = "decimal(15,8)")]

        public decimal? Height { get; set; }

        public string LengthClassUnit { get; set; }

        public List<ProductDiscount> ProductDiscounts { get; set; } = new List<ProductDiscount>();

    }

}


