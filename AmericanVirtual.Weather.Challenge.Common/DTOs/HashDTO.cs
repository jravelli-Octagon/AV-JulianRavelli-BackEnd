using AmericanVirtual.Weather.Challenge.Common.Extensions;

namespace AmericanVirtual.Weather.Challenge.Common.DTOs
{
    public class HashDTO
    {
        public string Username { get; set; }
        public long TimeStamp { get; set; }
        public string RandomString { get; set; }

        public HashDTO(string username)
        {
            Username = username;
            TimeStamp = DateTime.Now.ToFileTime();
            RandomString = StringExtensions.GetRandomString(10);
        }
    }
}