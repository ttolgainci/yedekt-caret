namespace MarbleWebProject.Models
{
    public class AllInfoResponse
    {
        public string? Order { get; set; }
        public int ID { get; set; }
        public int InformationID { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Url { get; set; }
        public bool? Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string UpdatedBy { get; set; } = string.Empty;
        public string LanguageCode { get; set; }
        public string Description { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeyword { get; set; }
        public List<FaqInfoModel> FaqDescriptions { get; set; }
    }
    public class FaqInfoModel
    {
        public string Name { get; set; }
        public string Order { get; set; }
        public List<FaqDetailInfoModel> Details { get; set; }
    }
    public class FaqDetailInfoModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Order { get; set; }
    }
}
