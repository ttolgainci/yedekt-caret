namespace MarbleWebProject.Models
{
    public class CategoryRouteModel
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public string LanguageCode { get; set; }
        public int ID { get; set; }
        public int CatID { get; set; }
    }
    public class InformationRouteModel
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public string LanguageCode { get; set; }
        public string Type { get; set; }
    }
}
