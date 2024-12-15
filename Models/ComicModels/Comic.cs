using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackerWasm.Models.ComicModels;

public class Comic
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Read chapter should be greater than 0.")]
    public int? ChapterRead { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Total chapter should be greater than 0.")]
    public int? TotalChapter { get; set; }

    [ForeignKey(nameof(PublishingStatus))]
    public int? PublishingStatusId { get; set; }

    public virtual PublishingStatus? PublishingStatus { get; set; }

    [ForeignKey(nameof(ReadStatus))]
    public int? ReadStatusId { get; set; }

    public virtual ReadStatus? ReadStatus { get; set; }

    [ForeignKey(nameof(ComicType))]
    public int? ComicTypeId { get; set; }
    public virtual ComicType? ComicType { get; set; }
}