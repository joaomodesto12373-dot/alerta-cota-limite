using System;
using AlertaCotaLimite.Models;

namespace AlertaCotaLimite.Services
{
    public class ConfigService
    {
        public Config LoadConfig()
        {
            try
            {
                string email = Environment.GetEnvironmentVariable("ALERT_EMAIL");
                string smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER");
                string smtpPortStr = Environment.GetEnvironmentVariable("SMTP_PORT");
                string smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME");
                string smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(smtpServer) || 
                    string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    throw new InvalidOperationException("Variáveis de ambiente não configuradas. Defina: ALERT_EMAIL, SMTP_SERVER, SMTP_PORT, SMTP_USERNAME, SMTP_PASSWORD");
                }

                if (!int.TryParse(smtpPortStr, out int smtpPort))
                {
                    throw new InvalidOperationException("SMTP_PORT deve ser um número inteiro");
                }

                Config config = new Config
                {
                    Email = email,
                    Smtp = new SmtpConfig
                    {
                        Server = smtpServer,
                        Port = smtpPort,
                        Username = smtpUsername,
                        Password = smtpPassword
                    }
                };

                return config;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar configuração: {ex.Message}");
                throw;
            }
        }
    }
}

