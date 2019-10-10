using IEvangelist.SignalR.Chat.Extensions;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IEvangelist.SignalR.Chat.Services
{
    public class TranslationService : ITranslationService
    {
        readonly HttpClient _httpClient;

        public TranslationService(HttpClient httpClient) => _httpClient = httpClient;

        public async ValueTask<(string text, bool isTranslated)> TranslateAsync(string text, string lang)
        {
            var response =
                await _httpClient.PostAsync(
                    $"/translate?api-version=3.0&scope=translation&to={lang}",
                    new StringContent(new object[] { new { text } }.ToJson(), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = json.FromJson<List<TranslationResult>>();

                return (result[0].Translations[0].Text, true);
            }

            return (text, false);
        }
    }

    public class TranslationResult
    {
        public DetectedLanguage DetectedLanguage { get; set; }
        public TextResult SourceText { get; set; }
        public Translation[] Translations { get; set; }
    }

    public class DetectedLanguage
    {
        public string Language { get; set; }
        public float Score { get; set; }
    }

    public class TextResult
    {
        public string Text { get; set; }
        public string Script { get; set; }
    }

    public class Translation
    {
        public string Text { get; set; }
        public TextResult Transliteration { get; set; }
        public string To { get; set; }
        public Alignment Alignment { get; set; }
        public SentenceLength SentLen { get; set; }
    }

    public class Alignment
    {
        public string Proj { get; set; }
    }

    public class SentenceLength
    {
        public int[] SrcSentLen { get; set; }
        public int[] TransSentLen { get; set; }
    }
}