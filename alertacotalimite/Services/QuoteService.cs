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
                    Console.WriteLine($"Aviso: Preço não encontrado na resposta da API para o símbolo {symbol}.");
                    return 0; // Retorna 0 em vez de lançar exceção
                }

                return priceToken.Value<decimal>();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Aviso: Erro de requisição ao buscar cotação (Rede/API): {e.Message}. Retornando 0.");
                return 0; // Retorna 0 em caso de falha de rede/API
            }
            catch (Exception e)
            {
                Console.WriteLine($"Aviso: Erro ao processar cotação: {e.Message}. Retornando 0.");
                return 0; // Retorna 0 em caso de outros erros
            }
        }
    }
}
