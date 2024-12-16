using System.Text;
using System.Text.Json;
using TrackerWasm.Models.ComicModels;

namespace TrackerWasm.Services;

public static class FirestoreDataService
{
    public static class ComicService
    {
        public static StringContent SerializeComic(Comic comic)
        {
            var payload = new
            {
                fields = new
                {
                    title = new { stringValue = comic.Title },
                    chapterRead = new { integerValue = comic.ChapterRead ?? 0 },
                    totalChapter = new { integerValue = comic.TotalChapter ?? 0 },
                    publishingStatus = new { stringValue = comic.PublishingStatus ?? string.Empty },
                    readStatus = new { stringValue = comic.ReadStatus ?? string.Empty },
                    comicType = new { stringValue = comic.ComicType ?? string.Empty }
                }
            };
            var json = JsonSerializer.Serialize(payload);
            Console.WriteLine(json);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        public static Comic DeserializeComic(JsonElement jsonElement)
        {
            var comic = new Comic();

            if (jsonElement.TryGetProperty("name", out var nameProperty))
            {
                var documentName = nameProperty.GetString() ?? string.Empty;
                var documentId = documentName.Split('/').Last();
                comic.Id = documentId;
            }

            if (!jsonElement.TryGetProperty("fields", out var fields)) return new Comic();

            if (fields.TryGetProperty("title", out var title))
            {
                comic.Title = title.GetProperty("stringValue").GetString() ?? string.Empty;
            }

            if (fields.TryGetProperty("chapterRead", out var chapterRead))
            {
                var chapterReadParsed =
                    int.Parse(chapterRead.GetProperty("integerValue").GetString() ?? string.Empty);
                comic.ChapterRead = chapterReadParsed > 0 ? chapterReadParsed : null;
            }

            if (fields.TryGetProperty("totalChapter", out var totalChapter))
            {
                var totalChapterParsed =
                    int.Parse(totalChapter.GetProperty("integerValue").GetString() ?? string.Empty);
                comic.TotalChapter = totalChapterParsed > 0 ? totalChapterParsed : null;
            }

            if (fields.TryGetProperty("publishingStatus", out var publishingStatus))
            {
                comic.PublishingStatus = publishingStatus.GetProperty("stringValue").GetString();
            }

            if (fields.TryGetProperty("readStatus", out var readStatus))
            {
                comic.ReadStatus = readStatus.GetProperty("stringValue").GetString();
            }

            if (fields.TryGetProperty("comicType", out var comicType))
            {
                comic.ComicType = comicType.GetProperty("stringValue").GetString();
            }
            
            return comic;
        }

        public static List<Comic> DeserializeComics(JsonElement jsonElement)
        {
            List<Comic> comics = [];
            if (!jsonElement.TryGetProperty("documents", out var documents)) return [];
            comics.AddRange(documents.EnumerateArray().Select(DeserializeComic));
            return comics;
        }
    }
}