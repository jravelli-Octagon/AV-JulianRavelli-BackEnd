using AmericanVirtual.Weather.Challenge.Database.Model;

namespace AmericanVirtual.Weather.Challenge.Database.Audit
{
    public class PartialAuditable : IPartialAuditable
    {
        public DateTime? CreationDate { get; set; }
        public User CreatedBy { get; set; }
    }
}