using System.ComponentModel.DataAnnotations;

namespace TrackerWasm.Models;

public class User
{
    [Required] public string Username { get; set; } = "";

    [Required]
    [Display(Name = "Display Name")]
    public string DisplayName { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Re-enter Password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string PasswordConfirmation { get; set; } = "";

    public string HashedPassword { get; set; } = "";

    public string UserId { get; set; } = "";
}