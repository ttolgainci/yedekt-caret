namespace MarbleWebProject.Models
{
    public class ProductInfoViewModel
    {
        public ProductInfo Product { get; set; } = new ProductInfo();
        public IReadOnlyList<ProductVariantListItemDto> Variants { get; set; } = Array.Empty<ProductVariantListItemDto>();
    }
}
