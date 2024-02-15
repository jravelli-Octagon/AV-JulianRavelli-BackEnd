namespace AmericanVirtual.Weather.Challenge.Common.DTOs
{
    public class UserDTO
    {
       
        public long ID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int FailedAttempts { get; set; }
        public DateTime PasswordChangeDate { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string CurrentState { get; set; } = string.Empty;
    }
}