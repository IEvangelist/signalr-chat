using BlazingChatter.Extensions;
using BlazingChatter.Records;
using System.Net;
using System.Text;

namespace BlazingChatter.Services;

internal sealed class TranslationService(
    HttpClient httpClient,
    ILogger<TranslationService> logger) : ITranslationService
{
    public async ValueTask<(string? text, bool isTranslated)> TranslateAsync(
        string text,
        string lang)
    {
        var target = NormalizeLang(lang);
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(target))
        {
            return (text, false);
        }

        try
        {
            // When an Azure Translator endpoint is configured the HttpClient has a
            // BaseAddress; otherwise we fall back to the free, key-less MyMemory API so
            // translation works out of the box. Either way, any failure returns the
            // original text so a joke is never swallowed by a translation error.
            return httpClient.BaseAddress is not null
                ? await TranslateWithAzureAsync(text, target)
                : await TranslateWithMyMemoryAsync(text, target);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex, "Translation to '{Lang}' failed; returning original text.", target);

            return (text, false);
        }
    }

    async ValueTask<(string? text, bool isTranslated)> TranslateWithAzureAsync(
        string text, string lang)
    {
        var response = await httpClient.PostAsync(
            $"/translate?api-version=3.0&scope=translation&to={lang}",
            new StringContent(
                new object[] { new { Text = text } }.ToJson() ?? "",
                Encoding.UTF8,
                "application/json"));

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var result = json.FromJson<List<TranslationApiResponse>>();
            var translated = result?[0]?.Translations?[0]?.Text;

            return (translated ?? text, translated is { Length: > 0 });
        }

        logger.LogWarning(
            "Azure translate returned {Status} for '{Lang}'.", response.StatusCode, lang);

        return (text, false);
    }

    async ValueTask<(string? text, bool isTranslated)> TranslateWithMyMemoryAsync(
        string text, string lang)
    {
        var url =
            $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}" +
            $"&langpair={Uri.EscapeDataString($"en|{lang}")}";

        var response = await httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode is false)
        {
            logger.LogWarning(
                "MyMemory translate returned {Status} for '{Lang}'.", response.StatusCode, lang);

            return (text, false);
        }

        var json = await response.Content.ReadAsStringAsync();
        var result = json.FromJson<MyMemoryResponse>();
        var translated = result?.ResponseData?.TranslatedText;

        if (result?.ResponseStatus is not 200 || string.IsNullOrWhiteSpace(translated))
        {
            logger.LogWarning("MyMemory could not translate to '{Lang}'.", lang);

            return (text, false);
        }

        // MyMemory HTML-encodes entities (e.g. &#39;) in the translated text.
        return (WebUtility.HtmlDecode(translated), true);
    }

    // MyMemory (and Azure) expect a language tag like "bg" or "es". Strip any region
    // suffix and normalize so callers can pass "es-ES", "bg", etc.
    static string NormalizeLang(string lang) =>
        string.IsNullOrWhiteSpace(lang)
            ? lang
            : lang.Trim().Split('-', StringSplitOptions.RemoveEmptyEntries) is { Length: > 0 } parts
                ? parts[0].ToLowerInvariant()
                : lang.Trim().ToLowerInvariant();
}