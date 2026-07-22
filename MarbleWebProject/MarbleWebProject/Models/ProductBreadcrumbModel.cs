namespace MarbleWebProject.Models
{
    public class ProductBreadcrumbModel
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public List<CategoryBreadcrumbModel> CagetoryList { get; set; } = new();
    }
}
