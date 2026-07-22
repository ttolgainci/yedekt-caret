namespace MarbleWebProject.Models
{
    /// <summary>Porto product-details-tab: Description + Additional information + Vehicle fitment.</summary>
    public class ProductDetailTabsViewModel
    {
        public string? Description { get; set; }
        public ProductInfo? Product { get; set; }
        public List<ProductAttributeList> Attributes { get; set; } = new();
        public List<ProductVehicleFitmentItem> VehicleFitments { get; set; } = new();
    }
}
