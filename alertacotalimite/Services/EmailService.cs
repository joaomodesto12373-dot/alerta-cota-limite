using AlertaCotaLimite.Models;
using System;
using System.Net;
using System.Net.Mail;

namespace AlertaCotaLimite.Services
{
    public class EmailService
    {
        private readonly Config _config;

        public EmailService(Config config)
        {
            _config = config;
        }

        public void SendAlert(string symbol, decimal currentPrice, string alertType)
        {
            try
            {
                using (SmtpClient client = new SmtpClient(_config.Smtp, _config.Port))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_config.Username, _config.Password);

                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(_config.Email);
                    mailMessage.To.Add(_config.Email); 
                    mailMessage.Subject = $"ALERTA DE COTAÇÃO: {alertType} para {symbol}";
                    mailMessage.Body = $"O ativo {symbol} atingiu o limite de {alertType} com o preço de R$ {currentPrice:F2} em {DateTime.Now}.";

                    client.Send(mailMessage);
                    Console.WriteLine($"Alerta de {alertType} enviado para {_config.Email} com sucesso.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
            }
        }
    }
}
