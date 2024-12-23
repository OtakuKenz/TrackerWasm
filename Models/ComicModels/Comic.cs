using System.ComponentModel.DataAnnotations;

namespace TrackerWasm.Models.ComicModels;

public class Comic
{
    public string Id { get; set; } = "";

    [Required] public string Title { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Read chapter should be greater than 0.")]
    [Display(Name = "Read chapter")]
    public int? ChapterRead { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Total chapter should be greater than 0.")]
    [Display(Name = "Total chapter")]
    public int? TotalChapter { get; set; }

    [Display(Name = "Publishing status")] public string? PublishingStatus { get; set; }

    [Display(Name = "Read status")] public string? ReadStatus { get; set; }

    [Display(Name = "Comic type")] public string? ComicType { get; set; }
}