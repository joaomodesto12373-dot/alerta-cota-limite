using System;
using System.Threading.Tasks;
using AlertaCotaLimite.Models;
using System.Globalization;


namespace AlertaCotaLimite.Services
{
    public class AlertMonitor
    {
        private readonly QuoteService _quoteService;
        private readonly EmailService _emailService;
        private readonly string _symbol;
        private readonly decimal _buyPrice;
        private readonly decimal _sellPrice;
        private decimal _lastPrice;
        private bool _buyAlertSent = false;
        private bool _sellAlertSent = false;

        public AlertMonitor(QuoteService quoteService, EmailService emailService, string symbol, decimal buyPrice, decimal sellPrice)
        {
            _quoteService = quoteService;
            _emailService = emailService;
            _symbol = symbol;
            _buyPrice = buyPrice;
            _sellPrice = sellPrice;
            _lastPrice = 0;
        }

        public async Task StartMonitoring(int intervalSeconds = 30)
        {
            Console.WriteLine($"Iniciando monitoramento de {_symbol}...");
            Console.WriteLine($"Alerta de COMPRA quando preço < R$ {_buyPrice.ToString("F2", CultureInfo.InvariantCulture)}");
            Console.WriteLine($"Alerta de VENDA quando preço > R$ {_sellPrice.ToString("F2", CultureInfo.InvariantCulture)}");

            Console.WriteLine("Pressione Ctrl+C para parar.\n");

            while (true)
            {
                try
                {
                    decimal currentPrice = await _quoteService.GetCurrentPrice(_symbol);
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {_symbol}: R$ {currentPrice:F2}");

                    CheckAndSendAlerts(currentPrice);

                    _lastPrice = currentPrice;
                    await Task.Delay(intervalSeconds * 1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro durante monitoramento: {ex.Message}");
                    await Task.Delay(intervalSeconds * 1000);
                }
            }
        }

        private void CheckAndSendAlerts(decimal currentPrice)
        {
            // Alerta de COMPRA (preço caiu abaixo do limite)
            if (currentPrice < _buyPrice && !_buyAlertSent)
            {
                _emailService.SendAlert(_symbol, currentPrice, "COMPRA");
                _buyAlertSent = true;
                _sellAlertSent = false;
                Console.WriteLine($"ALERTA DE COMPRA ENVIADO!");
            }

            // Alerta de VENDA (preço subiu acima do limite)
            if (currentPrice > _sellPrice && !_sellAlertSent)
            {
                _emailService.SendAlert(_symbol, currentPrice, "VENDA");
                _sellAlertSent = true;
                _buyAlertSent = false;
                Console.WriteLine($"ALERTA DE VENDA ENVIADO!");
            }

            // Reset de alertas quando volta para zona neutra
            if (currentPrice >= _buyPrice && currentPrice <= _sellPrice)
            {
                _buyAlertSent = false;
                _sellAlertSent = false;
            }
        }
    }
}
