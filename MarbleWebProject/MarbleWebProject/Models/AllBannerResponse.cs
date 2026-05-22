namespace MarbleWebProject.Models
{
    public class AllBannerResponse
    {
        public string? Order { get; set; }
        public int ID { get; set; }
        public int BannerID { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Url { get; set; }
        public bool? Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string UpdatedBy { get; set; } = string.Empty;
        public string LanguageCode { get; set; }
    }
}
