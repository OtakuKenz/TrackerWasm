using System.ComponentModel.DataAnnotations;

namespace TrackerWasm.Models.ComicModels;

public class PublishingStatus
{
    [Key] public int Id { get; set; }

    [Required] public required string Value { get; set; }
}