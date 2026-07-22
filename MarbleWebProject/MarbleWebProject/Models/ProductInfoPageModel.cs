namespace MarbleWebProject.Models
{
    public class ProductInfoPageModel
    {
        public ProductInfo Product { get; set; } = new();
        public bool HasVehicleFitment { get; set; }
    }
}
