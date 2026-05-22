using System.ComponentModel.DataAnnotations;

namespace MarbleWebProject.Models
{
    public class OrderBasket
    {
        [Key]
        public int ID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = String.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string UpdatedBy { get; set; } = String.Empty;
        public int ProductID { get; set; }
        public int? ProductVariantID { get; set; }
        public string UserID { get; set; }
        public string LanguageCode { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public int? quantity { get; set; }
        public decimal? Price { get; set; }
        public int? Tax { get; set; }
        public int? OrderID { get; set; }
        public string Currency { get; set; }
        public string Image { get; set; }
        public string Url { get; set; }
    }
}
