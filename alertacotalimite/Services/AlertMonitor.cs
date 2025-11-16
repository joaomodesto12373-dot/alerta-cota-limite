using AlertaCotaLimite.Models;
using System;
using System.Threading.Tasks;

namespace AlertaCotaLimite.Services
{
    public class AlertMonitor
    {
        // Dependências
        private readonly QuoteService _quoteService;
        private readonly EmailService _emailService;
        private readonly FileAlertPersistenceService _persistenceService;

        // Parâmetros do Monitoramento
        private readonly string _symbol;
        private readonly decimal _buyPrice;
        private readonly decimal _sellPrice;

        // Estado Persistente
        private AlertState _state; 

        // CONSTRUTOR CORRIGIDO PARA ACEITAR 6 ARGUMENTOS
        public AlertMonitor(QuoteService quoteService, EmailService emailService, FileAlertPersistenceService persistenceService, string symbol, decimal buyPrice, decimal sellPrice)
        {
            // 1. Injeta as dependências
            _quoteService = quoteService;
            _emailService = emailService;
            _persistenceService = persistenceService;

            // 2. Armazena os parâmetros de monitoramento
            _symbol = symbol;
            _buyPrice = buyPrice;
            _sellPrice = sellPrice;

            // 3. Carrega o estado persistente
            _state = _persistenceService.LoadState();
        }

        public async Task StartMonitoring(int intervalSeconds = 30)
        {
            Console.WriteLine($"Iniciando monitoramento de {_symbol}...");
            Console.WriteLine($"Alerta de COMPRA quando preço < R$ {_buyPrice:F2}");
            Console.WriteLine($"Alerta de VENDA quando preço > R$ {_sellPrice:F2}");
            Console.WriteLine("Pressione Ctrl+C para parar.\n");

            while (true)
            {
                try
                {
                    decimal currentPrice = await _quoteService.GetCurrentPrice(_symbol);
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {_symbol}: R$ {currentPrice:F2}");

                    CheckAndSendAlerts(currentPrice);

                    // Salva o estado após a verificação
                    _persistenceService.SaveState(_state); 

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
            // Lógica de Compra
            if (currentPrice < _buyPrice && !_state.BuyAlertSent)
            {
                _emailService.SendAlert(_symbol, currentPrice, "COMPRA");
                _state.BuyAlertSent = true;
                _state.SellAlertSent = false; // Reseta o alerta oposto
            }
            // Lógica de Venda
            else if (currentPrice > _sellPrice && !_state.SellAlertSent)
            {
                _emailService.SendAlert(_symbol, currentPrice, "VENDA");
                _state.SellAlertSent = true;
                _state.BuyAlertSent = false; // Reseta o alerta oposto
            }
            // Lógica de Reversão (Opcional: Reseta o alerta se o preço se afastar do limite)
            else if (currentPrice > _buyPrice && _state.BuyAlertSent)
            {
                _state.BuyAlertSent = false;
            }
            else if (currentPrice < _sellPrice && _state.SellAlertSent)
            {
                _state.SellAlertSent = false;
            }
            
            _state.LastPrice = currentPrice;
        }
    }
}
