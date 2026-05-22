namespace MarbleWebProject.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string? CorrelationId { get; set; }

        public bool ShowCorrelationId => !string.IsNullOrEmpty(CorrelationId);

        public string Heading { get; set; } = "Hata";

        public string Message { get; set; } = "İsteğiniz işlenirken bir sorun oluştu.";
    }
}
