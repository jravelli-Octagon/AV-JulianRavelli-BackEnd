using System.Runtime.Serialization;

namespace AmericanVirtual.Weather.Challenge.Common.DTOs
{
    public class CaptchaResponseDTO
    {
        [DataMember(Name = "success")]

        public bool Success { get; set; }
        [DataMember(Name = "challenge_ts")]

        public DateTime ChallengeTimeStamp { get; set; }
        [DataMember(Name = "hostname")]

        public string Hostname { get; set; } = string.Empty;
        [DataMember(Name = "error-codes")]

        public IEnumerable<string>? ErrorCodes { get; set; }
    }
}