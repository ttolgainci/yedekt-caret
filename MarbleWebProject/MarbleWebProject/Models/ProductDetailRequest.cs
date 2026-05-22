namespace MarbleWebProject.Models
{
    public class ProductDetailRequest
    {
        //public string Url { get; set; }
        public string Language { get; set; }
        public int ID { get; set; }
        public string? UserID { get; set; }
    }
    public class ProductBreadcrumbRequest
    {
        public string CategoryUrl { get; set; }
        public string LanguageCode { get; set; }
    }
}
