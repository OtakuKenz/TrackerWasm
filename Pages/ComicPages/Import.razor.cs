using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TrackerWasm.Models.ComicModels;

namespace TrackerWasm.Pages.ComicPages;

public partial class Import : ComponentBase
{



    private class JsonMapping
    {
        [Required] public string Title { get; set; } = string.Empty;
        public string ComicType { get; set; } = string.Empty;
        public string PublishingStatus { get; set; } = string.Empty;
        public string ReadStatus { get; set; } = string.Empty;
        public string ChapterRead { get; set; } = string.Empty;
        public string TotalChapter { get; set; } = string.Empty;

        public JsonDocument JsonDocument { get; set; } = default!;
    }

    private class ConversionResult<T>
    {
        public bool IsValid { get; set; } = false;
        public T? Value { get; set; }
    }

    private ConversionResult<string> ConvertToString(JsonElement jsonElement, string property)
    {
        var result = new ConversionResult<string>();
        if (string.IsNullOrWhiteSpace(property))
        {
            return result;
        }

        if (jsonElement.TryGetProperty(property, out var element) && element.ValueKind == JsonValueKind.String)
        {
            var convertedString = element.GetString();
            result.IsValid = true;
            result.Value = convertedString?.Trim();
        }

        return result;
    }

    private ConversionResult<int> ConvertToInt(JsonElement jsonElement, string property)
    {
        var result = new ConversionResult<int>();
        if (string.IsNullOrWhiteSpace(property))
        {
            return result;
        }

        if (jsonElement.TryGetProperty(property, out var element) && element.ValueKind == JsonValueKind.Number)
        {
            var convertedInt = element.GetInt32();
            result.IsValid = true;
            result.Value = convertedInt;
        }

        return result;
    }

    private class ExpectedResponse
    {
        public int duplicate { get; set; }

        public int failedToImport { get; set; }
        public int comicsSaved { get; set; }
    }
}