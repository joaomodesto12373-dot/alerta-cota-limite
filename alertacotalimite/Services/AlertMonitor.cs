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
            
            // NOVO: Aviso de intervalo
            Console.WriteLine($"A API será consultada a cada {intervalSeconds} segundos.");

            // NOVO: Aviso de último preço salvo
            if (_state.LastPrice > 0)
            {
                Console.WriteLine($"Último preço salvo na memória: R$ {_state.LastPrice:F2}.");
            }
            
            Console.WriteLine("Pressione Ctrl+C para parar.\n");

            while (true)
            {
                try
                {
                    decimal currentPrice = await _quoteService.GetCurrentPrice(_symbol);
                    
                    // NOVO: Se o preço for 0 (falha de API), apenas espera e tenta novamente
                    if (currentPrice == 0)
                    {
                        await Task.Delay(intervalSeconds * 1000);
                        continue;
                    }

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {_symbol}: R$ {currentPrice:F2}");

                    CheckAndSendAlerts(currentPrice);

                    // Salva o estado após a verificação
                    _persistenceService.SaveState(_state); 

                    await Task.Delay(intervalSeconds * 1000);
                }
                catch (Exception ex)
                {
                    // O tratamento de erro principal está no QuoteService, mas mantemos este para erros inesperados
                    Console.WriteLine($"Erro inesperado durante monitoramento: {ex.Message}");
                    await Task.Delay(intervalSeconds * 1000);
                }
            }
        }

        private void CheckAndSendAlerts(decimal currentPrice)
        {
            // Lógica de Novo Alerta por Mudança de Limite (COMPRA)
            // Se o alerta já foi enviado E o novo limite de compra for mais agressivo (menor) que o último preço que disparou o alerta,
            // então o alerta é reativado (BuyAlertSent = false).
            if (_state.BuyAlertSent && _buyPrice < _state.LastBuyAlertPrice)
            {
                _state.BuyAlertSent = false;
            }

            // Lógica de Novo Alerta por Mudança de Limite (VENDA)
            // Se o alerta já foi enviado E o novo limite de venda for mais agressivo (maior) que o último preço que disparou o alerta,
            // então o alerta é reativado (SellAlertSent = false).
            if (_state.SellAlertSent && _sellPrice > _state.LastSellAlertPrice)
            {
                _state.SellAlertSent = false;
            }


            // Lógica de Compra
            if (currentPrice < _buyPrice && !_state.BuyAlertSent)
            {
                _emailService.SendAlert(_symbol, currentPrice, "COMPRA");
                _state.BuyAlertSent = true;
                _state.SellAlertSent = false; // Reseta o alerta oposto
                _state.LastBuyAlertPrice = _buyPrice; // NOVO: Salva o limite que disparou o alerta
            }
            // Lógica de Venda
            else if (currentPrice > _sellPrice && !_state.SellAlertSent)
            {
                _emailService.SendAlert(_symbol, currentPrice, "VENDA");
                _state.SellAlertSent = true;
                _state.BuyAlertSent = false; // Reseta o alerta oposto
                _state.LastSellAlertPrice = _sellPrice; // NOVO: Salva o limite que disparou o alerta
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
