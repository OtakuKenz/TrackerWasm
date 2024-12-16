using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace TrackerWasm.Models;

public class ImportJsonMapping
{
    [Required] public string Title { get; set; } = string.Empty;
    public string ComicType { get; set; } = string.Empty;
    public string PublishingStatus { get; set; } = string.Empty;
    public string ReadStatus { get; set; } = string.Empty;
    public string ChapterRead { get; set; } = string.Empty;
    public string TotalChapter { get; set; } = string.Empty;

    public JsonDocument JsonDocument { get; set; } = default!;
}