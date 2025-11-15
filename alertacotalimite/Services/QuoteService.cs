using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AlertaCotaLimite.Services
{
    public class QuoteService
    {
        private readonly HttpClient _httpClient;
        private const string BrapiUrl = "https://brapi.dev/api/quote/";

        public QuoteService( )
        {
            _httpClient = new HttpClient( );
        }

        public async Task<decimal> GetCurrentPrice(string symbol)
        {
            try
            {
                string url = $"{BrapiUrl}{symbol}";
                HttpResponseMessage response = await _httpClient.GetAsync(url );

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro ao buscar cotação: {response.StatusCode}");
                }

                string content = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(content);

                decimal price = json["results"][0]["regularMarketPrice"].Value<decimal>();
                return price;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter preço de {symbol}: {ex.Message}");
                throw;
            }
        }
    }
}
