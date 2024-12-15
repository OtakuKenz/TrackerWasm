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
    [Inject] protected ToastService ToastService { get; set; } = default!;

    [Inject] protected PreloadService PreloadService { get; set; } = default!;

    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

    [Inject] public HttpClient Http { get; set; } = default!;

    private List<string> jsonProperties = []; // List of JSON properties
    private Dictionary<string, string> propertyMappings = []; // Map of JSON field to model property

    private JsonMapping jsonMapping = new();

    private JsonDocument? jsonDocument;

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

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        {
            await using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            // Read the content of the file
            var jsonContent = await reader.ReadToEndAsync();

            try
            {
                jsonDocument = JsonDocument.Parse(jsonContent);

                JsonElement root = jsonDocument.RootElement;

                var firstElement = jsonDocument.RootElement.EnumerateArray().First();
                foreach (JsonProperty property in firstElement.EnumerateObject())
                {
                    jsonProperties.Add(property.Name);
                }
            }
            catch (JsonException ex)
            {
                ToastService.Notify(new ToastMessage(ToastType.Danger, $"Invalid JSON format: {ex.Message}"));
            }
        }
    }

    private async Task ProcessJson()
    {
        PreloadService.Show();
        var comicsSaved = 0;
        var failedToImport = 0;
        var duplicate = 0;
        if (jsonDocument == null || string.IsNullOrWhiteSpace(jsonMapping.Title))
        {
            PreloadService.Hide();
            return;
        }

        jsonMapping.JsonDocument = jsonDocument;

        var response = await Http.PostAsJsonAsync("api/Comic/import", jsonMapping);

        if (response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadFromJsonAsync<ExpectedResponse>();
            if (body != null)
            {
                duplicate = body.duplicate;
                failedToImport = body.failedToImport;
                comicsSaved = body.comicsSaved;
            }

            PreloadService.Hide();
            if (comicsSaved == 0 && failedToImport > 0)
            {
                ToastService.Notify(new ToastMessage(ToastType.Danger, $"Failed to import all {failedToImport}."));
            }
            else if (failedToImport > 0 && comicsSaved > 0)
            {
                ToastService.Notify(
                    new ToastMessage(
                        ToastType.Warning,
                        $"{comicsSaved} records imported. {failedToImport} not imported."
                    )
                );
            }
            else if (failedToImport == 0 && comicsSaved > 0)
            {
                ToastService.Notify(new ToastMessage(ToastType.Success, $"{comicsSaved} records imported."));
            }

            if (duplicate > 0)
            {
                var message = "All records are duplicate. Nothing to import.";
                if (comicsSaved  > 0 || failedToImport > 0)
                {
                    message = $"{duplicate} {(duplicate > 1 ? "duplicates" : "duplicate")} skipped.";
                }

                ToastService.Notify(
                    new ToastMessage(
                        ToastType.Info,
                        message
                    )
                );
            }

            if (comicsSaved > 0)
            {
                NavigationManager.NavigateTo("/comic/home");
            }
        }
        else
        {
            ToastService.Notify(
                new ToastMessage(
                    ToastType.Danger,
                    $"Failed to import."
                ));
        }
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