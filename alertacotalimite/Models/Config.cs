namespace AlertaCotaLimite.Models 
{
    public class Config 
    {
        public string Email { get; set; }
        public SmtpConfig Smtp { get; set; }
    }

    public class SmtpConfig
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
