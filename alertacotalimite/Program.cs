using System;
using System.Threading.Tasks;
using AlertaCotaLimite.Models;
using AlertaCotaLimite.Services;
using System.Globalization;

namespace AlertaCotaLimite
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                if (args.Length < 3 || args.Length > 4)
                {
                    Console.WriteLine("Uso: dotnet run <SYMBOL> <SELL_PRICE> <BUY_PRICE> [reset]");
                    Console.WriteLine("Exemplo: dotnet run PETR4 22.67 22.59");
                    Console.WriteLine("Exemplo com reset: dotnet run PETR4 22.67 22.59 reset");
                    return;
                }

                string symbol = args[0];
                if (!decimal.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price1))
                {
                    Console.WriteLine("Erro: Segundo argumento deve ser um número válido");
                    return;
                }
                if (!decimal.TryParse(args[2], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price2))
                {
                    Console.WriteLine("Erro: Terceiro argumento deve ser um número válido");
                    return;
                }

                // qualquer ordem
                decimal buyPrice = Math.Min(price1, price2);
                decimal sellPrice = Math.Max(price1, price2);

                Console.WriteLine($"Preço de COMPRA (mínimo): R$ {buyPrice:F2}");
                Console.WriteLine($"Preço de VENDA (máximo): R$ {sellPrice:F2}\n");

                // Carregar configuração
                ConfigService configService = new ConfigService();
                Config config = configService.LoadConfig();

                // Criar serviços
                QuoteService quoteService = new QuoteService();
                EmailService emailService = new EmailService(config);

                // v3
                FileAlertPersistenceService persistenceService = new FileAlertPersistenceService(); 

                // Verificar reset foi passado
                if (args.Length == 4 && args[3].Equals("reset", StringComparison.OrdinalIgnoreCase))
                {
                    persistenceService.ResetState();
                }

                // Criar monitor e iniciar
                AlertMonitor monitor = new AlertMonitor(quoteService, emailService, persistenceService, symbol, buyPrice, sellPrice);
                await monitor.StartMonitoring(30); // 30s
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro fatal: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}