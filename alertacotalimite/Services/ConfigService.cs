using AlertaCotaLimite.Models;
using System;
using System.IO;
using System.Text.Json;

namespace AlertaCotaLimite.Services
{
    public class ConfigService
    {
        // Força a leitura de todas as configurações do appsettings.json
        public Config LoadConfig(string configPath = "appsettings.json")
        {
            if (!File.Exists(configPath))
            {
                // Erro fatal se o arquivo não for encontrado, conforme o requisito de leitura de arquivo.
                throw new FileNotFoundException($"Erro fatal: Arquivo de configuração não encontrado: {configPath}");
            }

            try
            {
                string jsonString = File.ReadAllText(configPath);
                
                // Deserializa o JSON. O operador ?? new Config() garante que o objeto não seja nulo.
                Config config = JsonSerializer.Deserialize<Config>(jsonString) ?? new Config();

                // Verifica se as credenciais essenciais foram lidas
                if (string.IsNullOrEmpty(config.Email) || string.IsNullOrEmpty(config.Smtp) || string.IsNullOrEmpty(config.Username) || string.IsNullOrEmpty(config.Password))
                {
                    throw new Exception("Configuração de e-mail incompleta. Verifique se todos os campos (Email, Smtp, Username, Password) estão preenchidos no appsettings.json.");
                }

                return config;
            }
            catch (JsonException ex)
            {
                throw new Exception($"Erro ao processar o arquivo de configuração JSON: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao carregar configuração: {ex.Message}");
            }
        }
    }
}
