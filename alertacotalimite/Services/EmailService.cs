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
                    mailMessage.IsBodyHtml = true;
                    mailMessage.Body = $@"
                        <html>
                        <body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
                            <div style=""max-width: 600px; margin: 20px auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;"">
                                <h2 style=""color: #007bff; border-bottom: 2px solid #007bff; padding-bottom: 10px;"">ALERTA DE COTAÇÃO: {alertType} para {symbol}</h2>
                                <p>Prezado(a) investidor(a),</p>
                                <p>Seu monitoramento de cotação foi acionado!</p>
                                <p style=""font-size: 18px; margin: 20px 0; padding: 15px; background-color: #f8f9fa; border-left: 5px solid #28a745;"">
                                    O ativo <strong>{symbol}</strong> atingiu o limite de <strong>{alertType}</strong>.
                                </p>
                                <p style=""font-size: 24px; font-weight: bold; color: #dc3545; text-align: center;"">
                                    Preço Atual: R$ {currentPrice:F2}
                                </p>
                                <p style=""text-align: right; font-size: 12px; color: #6c757d;"">
                                    Data e Hora: {DateTime.Now}
                                </p>
                                <p>Atenciosamente,</p>
                                <p>Seu Monitor de Alertas</p>
                            </div>
                        </body>
                        </html>";

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
