using System.Text.Json;
using static System.Text.Json.JsonSerializer;

namespace BlazingChatter.Extensions;

public static class ObjectExtensions
{
    static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static string? ToJson(this object value) =>
        value is null ? null : Serialize(value, _options);

    public static T? FromJson<T>(this string? json) =>
        string.IsNullOrWhiteSpace(json) ? default : Deserialize<T>(json, _options);
}