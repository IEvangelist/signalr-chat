namespace BlazingChatter.Records
{
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
}
