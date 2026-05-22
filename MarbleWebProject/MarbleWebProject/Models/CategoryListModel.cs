namespace MarbleWebProject.Models
{
    public class CategoryListModel
    {
        public string? Order { get; set; }
        public string? Picture { get; set; }
        public bool Status { get; set; }
        public int ID { get; set; }
        public int LanguageID { get; set; }
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public string? Description { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaKeyword { get; set; }
        public string? MetaDescription { get; set; }
        public string Url { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string UpdatedBy { get; set; } = string.Empty;
        public List<CategoryListModel> SubCat { get; set; } = new List<CategoryListModel>();
    }
    public class HeaderLink
    {
        public int MyProperty { get; set; }
    }
    public class MobileMenuModel
    {
        public List<CategoryListModel> Menu { get; set; } = new List<CategoryListModel>();
        public List<HeaderLink> HeaderLink { get; set; } = new List<HeaderLink>();
    }
}
