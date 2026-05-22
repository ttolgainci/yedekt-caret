namespace MarbleWebProject.Models
{
    public class ProductsByCageoryRequest
    {
        //public string Url { get; set; }
        public string LanguageCode { get; set; }
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public int ID { get; set; }
        public int CategoryID { get; set; }
    }
}
