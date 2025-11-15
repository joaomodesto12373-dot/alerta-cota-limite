using System;
using System.Net;
using System.Net.Mail;
using AlertaCotaLimite.Models;

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
                using (SmtpClient smtpClient = new SmtpClient(_config.Smtp.Server, _config.Smtp.Port))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.Credentials = new NetworkCredential(_config.Smtp.Username, _config.Smtp.Password);

                    string subject = $"Alerta de Cotação: {symbol}";
                    string body = $"O ativo {symbol} atingiu o preço de R$ {currentPrice:F2}.\n" +
                                  $"Tipo de alerta: {alertType}";

                    MailMessage mailMessage = new MailMessage(_config.Smtp.Username, _config.Email, subject, body);

                    smtpClient.Send(mailMessage);
                    Console.WriteLine($"E-mail enviado com sucesso para {_config.Email}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
                throw;
            }
        }
    }
}
