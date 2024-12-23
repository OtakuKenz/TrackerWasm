using System.ComponentModel.DataAnnotations;

namespace TrackerWasm.ViewModels;

public class Login
{
    [Required] public string Username { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";
}