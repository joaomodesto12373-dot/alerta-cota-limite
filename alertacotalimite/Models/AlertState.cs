namespace AlertaCotaLimite.Models
{
    public class AlertState
    {
        public bool BuyAlertSent { get; set; } = false;
        
        public bool SellAlertSent { get; set; } = false;
        
        public decimal LastPrice { get; set; } = 0;
    }
}
