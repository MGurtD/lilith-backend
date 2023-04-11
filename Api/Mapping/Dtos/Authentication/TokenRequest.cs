using System.ComponentModel.DataAnnotations;

public class TokenRequest 
{
    [Required]
    public string Token {get; set;} = null!;
    [Required]
    public Guid RefreshToken {get; set;}
}