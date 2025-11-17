namespace AlertaCotaLimite.Models
{
    public class AlertState
    {
        public bool BuyAlertSent { get; set; } = false;
        public bool SellAlertSent { get; set; } = false;
        public decimal LastPrice { get; set; } = 0;
        
        // NOVO: Armazena o preço que disparou o último alerta de compra
        public decimal LastBuyAlertPrice { get; set; } = 0; 
        
        // NOVO: Armazena o preço que disparou o último alerta de venda
        public decimal LastSellAlertPrice { get; set; } = 0; 
    }
}
