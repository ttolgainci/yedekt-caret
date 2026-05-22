namespace MarbleWebProject.Models
{
    public class BasketRequest
    {
        public int? ProductID { get; set; }
        public int? ProductVariantID { get; set; }
        public string? UserID { get; set; }
        public string? LanguageCode { get; set; }
        public int? CartQuantity { get; set; }
        public string? Url { get; set; }
    }
}
