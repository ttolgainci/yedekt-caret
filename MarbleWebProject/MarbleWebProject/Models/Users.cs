namespace MarbleWebProject.Models
{
    public class Users
    {
        public int ID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = String.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string UpdatedBy { get; set; } = String.Empty;
        public string Name { get; set; } = String.Empty;
        public string Surname { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public string RePassword { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string UserName { get; set; } = String.Empty;
        public int LanguageID { get; set; }
    }
}
