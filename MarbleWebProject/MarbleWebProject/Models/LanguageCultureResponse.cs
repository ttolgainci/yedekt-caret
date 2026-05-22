namespace MarbleWebProject.Models
{
    public class LanguageCultureResponse
    {
        public int ID { get; set; }
        public string? stCode { get; set; }
        public string? stName { get; set; }
        public string? stCulture { get; set; }
        public string? stCreatedUser { get; set; }
        public DateTime dtCreatedDate { get; set; }
        public string? stUpdatedUser { get; set; }
        public DateTime dtUpdatedDate { get; set; }
    }
}
