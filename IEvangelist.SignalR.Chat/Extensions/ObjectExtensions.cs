using static Newtonsoft.Json.JsonConvert;

namespace IEvangelist.SignalR.Chat.Extensions
{
    public static class ObjectExtensions
    {
        public static T FromJson<T>(this string json) => string.IsNullOrWhiteSpace(json) ? default : DeserializeObject<T>(json);
        public static string ToJson(this object value) => value is null ? null : SerializeObject(value);
    }
}