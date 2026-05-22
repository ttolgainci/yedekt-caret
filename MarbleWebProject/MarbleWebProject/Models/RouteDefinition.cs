namespace MarbleWebProject.Models
{
    public class RouteDefinition
    {
        public string Name { get; set; }
        public string Pattern { get; set; }
        public Info Defaults { get; set; }
    }
    public class Info
    {
        public string controller { get; set; }
        public string action { get; set; }
        public int id { get; set; }
        public int catID { get; set; }
    }
}
