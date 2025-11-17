using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AlertaCotaLimite.Services
{
    public class QuoteService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private const string ApiUrl = "https://brapi.dev/api/quote/";

        public async Task<decimal> GetCurrentPrice(string symbol)
        {
            try
            {
                string url = $"{ApiUrl}{symbol}?modules=summaryProfile&token=SUA_CHAVE_AQUI";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                
                JObject json = JObject.Parse(responseBody);
                
                JToken? priceToken = json["results"]?[0]?["regularMarketPrice"];

                if (priceToken == null)
                {
                    throw new Exception($"Preço não encontrado na resposta da API para o símbolo {symbol}.");
                }

                return priceToken.Value<decimal>();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Erro de requisição ao buscar cotação: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro ao processar cotação: {e.Message}");
                throw;
            }
        }
    }
}
