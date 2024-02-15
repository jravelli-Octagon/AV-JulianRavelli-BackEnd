using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace AmericanVirtual.Weather.Challenge.Database.Model
{ 

    [Table("AuthenticationTokens")]
    public class AuthenticationToken
    {
        [Key]
        public long ID { get; set; }
        public long UserID { get; set; }
        public virtual User User { get; set; }
        public DateTime CreationDate { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        
        public AuthenticationToken()
        {

        }

        public AuthenticationToken(User user, string token)
        {
            User = user;
            CreationDate = DateTime.Now;
            AccessToken = token;
            RefreshToken = GenerateRefreshToken();
            RefreshTokenExpiryTime = CreationDate.AddHours(8); // AddMinutes(10) para probar
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }


    }
}