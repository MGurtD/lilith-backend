namespace Api.Mapping.Dtos.Authentication
{
    public class AuthResponse
    {
        public string Token { get; set; } = null!;
        public bool Result { get; set; }
        public List<string>? Errors { get; set; }
    }
}
