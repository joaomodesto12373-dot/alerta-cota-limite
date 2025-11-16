using System;
using System.IO;
using Newtonsoft.Json;
using AlertaCotaLimite.Models;

namespace AlertaCotaLimite.Services
{
    public class ConfigService
    {
        public Config LoadConfig(string configPath = "appsettings.json")
        {
            try
            {
                if (!File.Exists(configPath))
                {
                    throw new FileNotFoundException($"Arquivo de configuração não encontrado: {configPath}");
                }

                string json = File.ReadAllText(configPath);
                Config config = JsonConvert.DeserializeObject<Config>(json);

                if (config == null || string.IsNullOrEmpty(config.Email) || config.Smtp == null)
                {
                    throw new InvalidOperationException("Configuração inválida ou incompleta no arquivo");
                }

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