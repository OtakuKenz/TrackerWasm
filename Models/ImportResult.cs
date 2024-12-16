namespace TrackerWasm.Models;

public class ImportResult
{
    public int Duplicate { get; set; } = 0;
    public int Failed { get; set; } = 0;
    public int Saved { get; set; } = 0;
}