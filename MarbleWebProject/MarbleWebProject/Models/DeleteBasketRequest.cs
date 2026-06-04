namespace MarbleWebProject.Models
{
	public class DeleteBasketRequest
	{
		public int? ProductID { get; set; }
		public string UserID { get; set; } = string.Empty;
		public string LanguageCode { get; set; } = string.Empty;
	}
}
