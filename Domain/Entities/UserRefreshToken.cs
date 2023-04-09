namespace Domain.Entities
{
    public class UserRefreshToken : Entity
    {
        public Guid UserId { get; set; }
        public Guid JwtId { get; set; }
        public string Token { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
