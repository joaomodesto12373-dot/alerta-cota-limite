namespace AlertaCotaLimite.Models
{
    public class Config
    {
        public string Server { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Smtp { get; set; } = string.Empty;
        public int Port { get; set; } = 0;
    }
}
