using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MarbleWebProject.Models
{
    public class ProductDetailResponse
    {
        public ProductInfo ProductDetails { get; set; } = new ProductInfo();
        public List<ProductAttributeList> ProductAttributes { get; set; } = new List<ProductAttributeList>();  
        public List<ProductImageList> ProductImages { get; set; }= new List<ProductImageList>();
        public ProductSimilarModel ProductSimilarData { get; set; }

        public int ProductID { get; set; }
    }
    public class ProductBreadcrumbsResponse
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public string Order { get; set; }
    }
}
