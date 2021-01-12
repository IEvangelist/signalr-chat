using BlazingChatter.Extensions;
using BlazingChatter.Records;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BlazingChatter.Services
{
    public class TranslationService : ITranslationService
    {
        readonly HttpClient _httpClient;
        readonly ILogger<TranslationService> _logger;

        public TranslationService(
            HttpClient httpClient, ILogger<TranslationService> logger) =>
            (_httpClient, _logger) = (httpClient, logger);

        public async ValueTask<(string text, bool isTranslated)> TranslateAsync(
            string text,
            string lang)
        {
            var response =
                await _httpClient.PostAsync(
                    $"/translate?api-version=3.0&scope=translation&to={lang}",
                    new StringContent(new object[] { new { Text = text } }.ToJson(),
                    Encoding.UTF8,
                    "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = json.FromJson<List<TranslationApiResponse>>();

                return (result?[0]?.Translations?[0]?.Text, true);
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Unable to translate: {json}");
            }

            return (text, false);
        }
    }
}