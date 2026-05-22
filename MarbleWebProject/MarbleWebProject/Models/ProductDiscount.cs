using System.ComponentModel.DataAnnotations.Schema;

namespace MarbleWebProject.Models
{
    public class ProductDiscount
    {
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public int Priority { get; set; }
        [Column(TypeName = "decimal(15,4)")]
        public decimal? price { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [NotMapped]
        public string SDate { get; set; }
        [NotMapped]
        public string EDate { get; set; }
    }
}
