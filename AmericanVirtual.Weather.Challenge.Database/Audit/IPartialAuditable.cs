using AmericanVirtual.Weather.Challenge.Database.Model;

namespace AmericanVirtual.Weather.Challenge.Database.Audit
{
    public interface IPartialAuditable
    {
        DateTime? CreationDate { get; set; }
        User CreatedBy { get; set; }
    }
}