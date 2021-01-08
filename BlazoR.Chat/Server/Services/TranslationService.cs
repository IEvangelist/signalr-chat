using BlazorR.Chat.Extensions;
using BlazorR.Chat.Records;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BlazorR.Chat.Services
{
    public class TranslationService : ITranslationService
    {
        readonly HttpClient _httpClient;

        public TranslationService(HttpClient httpClient) =>
            _httpClient = httpClient;

        public async ValueTask<(string text, bool isTranslated)> TranslateAsync(
            string text,
            string lang)
        {
            var response =
                await _httpClient.PostAsync(
                    $"/translate?api-version=3.0&scope=translation&to={lang}",
                    new StringContent(new object[] { new { text } }.ToJson(),
                    Encoding.UTF8,
                    "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = json.FromJson<List<TranslationApiResponse>>();

                return (result?[0]?.Translations?[0]?.Text, true);
            }

            return (text, false);
        }
    }
}