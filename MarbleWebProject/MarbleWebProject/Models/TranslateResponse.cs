namespace MarbleWebProject.Models
{
    public class TranslateResponse
    {
        public string Text { get; set; }
        public List<TranslateAllResponse> GetFullList { get; set; }
    }
    public class TranslateAllResponse
    {
        public int AgencyGroupID { get; set; }
        public string? Key { get; set; }
        public string? KeyLang { get; set; }
        public string? RetLang { get; set; }
        public string? Translation { get; set; }
        public string? URL { get; set; }
    }
    public class SiteMapUrlModel
    {     
        public string? Type { get; set; }
        public string? Url { get; set; }
        public string LanguageCode { get; set; }
    }
}
