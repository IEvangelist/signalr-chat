namespace BlazingChatter.Records;

public record TranslationApiResponse(
    DetectedLanguage DetectedLanguage,
    TextResult SourceText,
    Translation[] Translations);

public record DetectedLanguage(string Language, float Score);

public record TextResult(string Text, string Script);

public record Translation(
    string Text,
    TextResult Transliteration,
    string To,
    Alignment Alignment,
    SentenceLength SentLen);

public record Alignment(string Proj);

public record SentenceLength(int[] SrcSentLen, int[] TransSentLen);

/// <summary>
/// Shape of a response from the free, key-less MyMemory translation API
/// (https://api.mymemory.translated.net/get?q=...&amp;langpair=en|xx). Used as the
/// default provider so translation works out of the box, with no Azure key required.
/// </summary>
public record MyMemoryResponse(
    MyMemoryMatch? ResponseData,
    int? ResponseStatus);

public record MyMemoryMatch(string? TranslatedText);
