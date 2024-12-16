namespace TrackerWasm.Models;

public class JsonConverterResult<T>
{
    public bool IsValid { get; set; } = false;
    public T? Value { get; set; }
}