namespace MarbleWebProject.Models
{
    public class AccountLoginRequest
    {
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        /// <summary>Tenant key (e.g. ASKAYEDEK).</summary>
        public string CustomName { get; set; } = "";
        /// <summary>"store" → Users table (vitrin servis hesabı).</summary>
        public string Audience { get; set; } = "store";
    }
}
