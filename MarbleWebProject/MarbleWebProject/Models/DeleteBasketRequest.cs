namespace MarbleWebProject.Models
{
	public class DeleteBasketRequest
	{
		public int? ProductID { get; set; }
        public int? ProductVariantID { get; set; }
        public string UserID { get; set; }
		public string LanguageCode { get; set; }
	}
}
