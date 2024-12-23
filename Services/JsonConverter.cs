using System.Text.Json;
using TrackerWasm.Models;

namespace TrackerWasm.Services;

public class JsonConverter
{
    public static JsonConverterResult<string> ConvertToString(JsonElement jsonElement, string property)
    {
        var result = new JsonConverterResult<string>();
        if (string.IsNullOrWhiteSpace(property)) return result;

        if (!jsonElement.TryGetProperty(property, out var element) ||
            element.ValueKind != JsonValueKind.String) return result;

        var convertedString = element.GetString();
        result.IsValid = true;
        result.Value = convertedString?.Trim();

        return result;
    }

    public static JsonConverterResult<int> ConvertToInt(JsonElement jsonElement, string property)
    {
        var result = new JsonConverterResult<int>();
        if (string.IsNullOrWhiteSpace(property)) return result;

        if (!jsonElement.TryGetProperty(property, out var element) ||
            element.ValueKind != JsonValueKind.Number) return result;

        var convertedInt = element.GetInt32();
        result.IsValid = true;
        result.Value = convertedInt;

        return result;
    }
}