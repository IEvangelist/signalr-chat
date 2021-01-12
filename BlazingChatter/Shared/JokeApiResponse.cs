namespace BlazingChatter.Records
{
    public record JokeApiResponse(string Type, Value Value);

    public class Value
    {
        public int Id { get; set; }

        public string? Joke
        {
            get => _joke;
            set => _joke = value?.Replace("&quot;", "\"");
        }
        private string? _joke;
    }
}
