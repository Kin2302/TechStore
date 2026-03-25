namespace Application.DTOs.Integration
{
    public class SmtpOptions
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string FromEmail { get; set; } = "";
        public string FromName { get; set; } = "TechStore";
        public bool UseSsl { get; set; } = false; // 465=true, 587=false + StartTls
    }
}