namespace MarbleWebProject.Models
{
    public class ResolveSeoPathRequest
    {
        public string LanguageCode { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }

    public class ResolveSeoPathResponse
    {
        public string PageType { get; set; } = "NOT_FOUND";
        public int EntityId { get; set; }
        public int? CatId { get; set; }
    }

    public class SitemapPathItemDto
    {
        public string Type { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public int? CatId { get; set; }
    }
}
