using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TrackerWasm.Models;
using TrackerWasm.Models.ComicModels;

namespace TrackerWasm.Services;

public class ComicService(HttpClient http, UserService userService)
{
    public async Task<bool> IsComicTitleDuplicate(string comicTitle)
    {
        var query = new
        {
            structuredQuery = new
            {
                select = new
                {
                    fields = new[] { new { fieldPath = "title" } }
                },
                from = new[] { new { collectionId = "comic" } },
                where = new
                {
                    fieldFilter = new
                    {
                        field = new { fieldPath = "title" },
                        op = "EQUAL",
                        value = new { stringValue = comicTitle }
                    }
                }
            }
        };
        var url = http.BaseAddress + "documents:runQuery";
        var jsonContent = new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json");
        var result = await http.PostAsync(url, jsonContent);
        var responseContent = await result.Content.ReadAsStringAsync();
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(responseContent);
        return jsonElement.EnumerateArray().Any(element => element.TryGetProperty("document", out _));
    }

    private async Task<string> GetComicUrl()
    {
        var userId = await userService.GetUserId();
        return $"documents/comic-{userId}";
    }

    public async Task<List<string>> GetComicTypeList()
    {
        List<string> data = [];
        var response = await http.GetFromJsonAsync<JsonElement>("documents/comicType");
        if (!response.TryGetProperty("documents", out var documents)) return data;
        foreach (var doc in documents.EnumerateArray())
        {
            if (!doc.TryGetProperty("fields", out var fields)) continue;
            if (!fields.TryGetProperty("Value", out var status)) continue;
            var stringValue = status.GetProperty("stringValue").GetString();
            if (stringValue != null) data.Add(stringValue);
        }

        return data;
    }

    public async Task<List<string>> GetPublishingStatusList()
    {
        List<string> data = [];
        var response = await http.GetFromJsonAsync<JsonElement>("documents/publishingStatus");
        if (!response.TryGetProperty("documents", out var documents)) return data;
        foreach (var doc in documents.EnumerateArray())
        {
            if (!doc.TryGetProperty("fields", out var fields)) continue;
            if (!fields.TryGetProperty("Status", out var status)) continue;
            var stringValue = status.GetProperty("stringValue").GetString();
            if (stringValue != null) data.Add(stringValue);
        }

        return data;
    }

    public async Task<List<string>> GetReadingStatusList()
    {
        List<string> data = [];
        var response = await http.GetFromJsonAsync<JsonElement>("documents/readStatus");
        if (!response.TryGetProperty("documents", out var documents)) return data;
        foreach (var doc in documents.EnumerateArray())
        {
            if (!doc.TryGetProperty("fields", out var fields)) continue;
            if (!fields.TryGetProperty("Status", out var status)) continue;
            var stringValue = status.GetProperty("stringValue").GetString();
            if (stringValue != null) data.Add(stringValue);
        }

        return data;
    }

    private async Task<List<Comic>> GetComicList()
    {
        var response = await http.GetFromJsonAsync<JsonElement>(await GetComicUrl());
        var data = FirestoreDataService.ComicService.DeserializeComics(response);
        return data;
    }

    public async Task<List<Comic>> GetComicList(Comic search)
    {
        var comicList = await GetComicList();
        if (!string.IsNullOrWhiteSpace(search.Title))
            comicList = comicList
                .Where(comic => comic.Title.Contains(search.Title, StringComparison.CurrentCultureIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(search.ComicType))
            comicList = comicList
                .Where(comic => comic.ComicType == search.ComicType).ToList();

        if (!string.IsNullOrWhiteSpace(search.PublishingStatus))
            comicList = comicList
                .Where(comic => comic.PublishingStatus == search.PublishingStatus).ToList();

        if (!string.IsNullOrWhiteSpace(search.ReadStatus))
            comicList = comicList
                .Where(comic => comic.ReadStatus == search.ReadStatus).ToList();

        return comicList;
    }

    public async Task<Comic> GetComic(string comicId)
    {
        var jsonElement = await http.GetFromJsonAsync<JsonElement>($"{await GetComicUrl()}/{comicId}");
        return FirestoreDataService.ComicService.DeserializeComic(jsonElement);
    }

    public async Task<HttpResponseMessage> SaveComic(Comic comic)
    {
        var serializedComic = FirestoreDataService.ComicService.SerializeComic(comic);
        return await http.PostAsync(await GetComicUrl(), serializedComic);
    }

    public async Task<HttpResponseMessage> UpdateComic(Comic comic, string comicId)
    {
        var serialized = FirestoreDataService.ComicService.SerializeComic(comic);
        return await http.PatchAsync($"{await GetComicUrl()}/{comicId}", serialized);
    }

    public async Task<HttpResponseMessage> DeleteComic(string comicId)
    {
        return await http.DeleteAsync($"{await GetComicUrl()}/{comicId}");
    }

    public async Task<ImportResult> ImportComic(ImportJsonMapping jsonMapping)
    {
        var comicTypeList = await GetComicTypeList();
        var publishingStatusList = await GetPublishingStatusList();
        var readStatusList = await GetReadingStatusList();
        var comicList = await GetComicList();

        var importResult = new ImportResult();

        var comicsToSave = new List<Comic>();

        using var json = jsonMapping.JsonDocument.RootElement.EnumerateArray();
        foreach (var record in json)
        {
            var comic = new Comic();

            var title = ConvertToString(record, jsonMapping.Title);
            if (title.IsValid && !string.IsNullOrWhiteSpace(title.Value))
            {
                // check if title already exist in pending to save
                var pendingToSave = comicsToSave.FirstOrDefault(x => x.Title == title.Value);
                if (pendingToSave != null)
                {
                    importResult.Duplicate++;
                    continue;
                }

                // check if title exist in database
                var result = comicList.FirstOrDefault(x => x.Title == title.Value);
                if (result != null)
                {
                    importResult.Duplicate++;
                    continue;
                }

                comic.Title = title.Value;
            }
            else
            {
                importResult.Failed++;
                continue;
            }

            var chapterRead = ConvertToInt(record, jsonMapping.ChapterRead);
            if (chapterRead.IsValid) comic.ChapterRead = chapterRead.Value > 0 ? chapterRead.Value : null;

            var totalChapter = ConvertToInt(record, jsonMapping.TotalChapter);
            if (totalChapter.IsValid) comic.TotalChapter = totalChapter.Value > 0 ? totalChapter.Value : null;

            var comicTypeValue = ConvertToString(record, jsonMapping.ComicType);
            if (comicTypeValue.IsValid && !string.IsNullOrWhiteSpace(comicTypeValue.Value))
            {
                var comicType = comicTypeList.FirstOrDefault(x => x == comicTypeValue.Value);
                if (comicType == null)
                {
                    importResult.Failed++;
                    continue;
                }

                comic.ComicType = comicType;
            }

            var publishingStatusValue = ConvertToString(record, jsonMapping.PublishingStatus);
            if (publishingStatusValue.IsValid && !string.IsNullOrWhiteSpace(publishingStatusValue.Value))
            {
                var publishingType = publishingStatusList.FirstOrDefault(x => x == publishingStatusValue.Value);
                if (publishingType == null)
                {
                    importResult.Failed++;
                    continue;
                }

                comic.PublishingStatus = publishingType;
            }

            var readStatusValue = ConvertToString(record, jsonMapping.ReadStatus);
            if (readStatusValue.IsValid && !string.IsNullOrWhiteSpace(readStatusValue.Value))
            {
                var readStatus = readStatusList.FirstOrDefault(x => x == readStatusValue.Value);
                if (readStatus == null)
                {
                    importResult.Failed++;
                    continue;
                }

                comic.ReadStatus = readStatus;
            }

            comicsToSave.Add(comic);
        }

        importResult.Saved = comicsToSave.Count;
        var saveTasks = comicsToSave.Select(SaveComic); // Create tasks for each save operation
        await Task.WhenAll(saveTasks);

        return importResult;
    }

    private static ConversionResult<string> ConvertToString(JsonElement jsonElement, string property)
    {
        var result = new ConversionResult<string>();
        if (string.IsNullOrWhiteSpace(property)) return result;

        if (!jsonElement.TryGetProperty(property, out var element) ||
            element.ValueKind != JsonValueKind.String) return result;

        var convertedString = element.GetString();
        result.IsValid = true;
        result.Value = convertedString?.Trim();

        return result;
    }

    private static ConversionResult<int> ConvertToInt(JsonElement jsonElement, string property)
    {
        var result = new ConversionResult<int>();
        if (string.IsNullOrWhiteSpace(property)) return result;

        if (!jsonElement.TryGetProperty(property, out var element) ||
            element.ValueKind != JsonValueKind.Number) return result;

        var convertedInt = element.GetInt32();
        result.IsValid = true;
        result.Value = convertedInt;

        return result;
    }

    private sealed class ConversionResult<T>
    {
        public bool IsValid { get; set; }
        public T? Value { get; set; }
    }
}