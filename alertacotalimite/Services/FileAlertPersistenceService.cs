using AlertaCotaLimite.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace AlertaCotaLimite.Services
{
    public class FileAlertPersistenceService
    {
        private const string StateFilePath = "alertState.json";

        public AlertState LoadState()
        {
            if (File.Exists(StateFilePath))
            {
                try
                {
                    string json = File.ReadAllText(StateFilePath);
                    return JsonConvert.DeserializeObject<AlertState>(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao carregar estado de alerta: {ex.Message}. Iniciando com estado padrão.");
                    return new AlertState();
                }
            }
            return new AlertState();
        }

        public void SaveState(AlertState state)
        {
            try
            {
                string json = JsonConvert.SerializeObject(state, Formatting.Indented);
                File.WriteAllText(StateFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar estado de alerta: {ex.Message}");
            }
        }
        public void ResetState()
        {
            if (File.Exists(StateFilePath))
            {
                File.Delete(StateFilePath);
                Console.WriteLine($"Memória de alertas resetada. Arquivo {StateFilePath} excluído.");
            }
            else
            {
                Console.WriteLine("Nenhum arquivo de estado encontrado para resetar.");
            }
        }
    }
}
