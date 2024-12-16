using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackerWasm.Models.ComicModels;

public class Comic
{
    public string Id { get; set; } = "";

    [Required]
    public string Title { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Read chapter should be greater than 0.")]
    public int? ChapterRead { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Total chapter should be greater than 0.")]
    public int? TotalChapter { get; set; }

    public string? PublishingStatus { get; set; }


    public string? ReadStatus { get; set; }


    public string? ComicType { get; set; }
}