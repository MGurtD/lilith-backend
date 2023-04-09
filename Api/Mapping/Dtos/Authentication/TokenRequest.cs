using System.ComponentModel.DataAnnotations;

public class TokenRequest 
{
    [Required]
    public string Token {get; set;} = null!;
    [Required]
    public string RefreshToken {get; set;} = null!;
}