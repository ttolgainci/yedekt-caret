namespace MarbleWebProject.Models
{
    public class ProductsByCageoryResponse
    {
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDesc { get; set; }
        public string MetaKeyword { get; set; }
        public string Url { get; set; }
        public int CategoryID { get; set; }
        public List<ProductList> ProductList { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageNumber { get; set; }
    }
    public class ProductList
    {
        public string ProductName { get; set; }
        public decimal? Price { get; set; }
        public int? CurrencyID { get; set; }
        public string CurrencyName { get; set; }
        public string Description { get; set; }
        public string StockStatus { get; set; }
        public List<ImageList> Images { get; set; }
        public string MainImage { get; set; }
        public string Url { get; set; }
        public int ProductID { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string StockStatusColor { get; set; }
    }
    public class ImageList
    {
        public string Image { get; set; }
    }
}
