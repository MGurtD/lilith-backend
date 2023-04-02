namespace Api.DTOs
{
    public class AuthResponse
    {
        public string Token { get; set; } = null!;
        public bool Result { get; set; }
        public List<string>? Errors { get; set; }
    }
}
