namespace MarbleWebProject.Models
{
    public class ProductDetailResponse
    {
        public ProductInfo ProductDetails { get; set; } = new ProductInfo();
        public List<ProductAttributeList> ProductAttributes { get; set; } = new List<ProductAttributeList>();
        public List<ProductImageList> ProductImages { get; set; } = new List<ProductImageList>();
        public ProductSimilarModel ProductSimilarData { get; set; }

        public int ProductID { get; set; }

        /// <summary>API <c>api/catalog/product/{id}/variants</c></summary>
        public List<ProductVariantListItemDto> ProductVariants { get; set; } = new List<ProductVariantListItemDto>();

        /// <summary>API <c>api/catalog/product/{id}/fitments</c> (oto yedek parça)</summary>
        public List<VehicleCompatibilityListItemDto> VehicleCompatibilities { get; set; } = new List<VehicleCompatibilityListItemDto>();
    }
    public class ProductBreadcrumbsResponse
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public string Order { get; set; }
    }
}
