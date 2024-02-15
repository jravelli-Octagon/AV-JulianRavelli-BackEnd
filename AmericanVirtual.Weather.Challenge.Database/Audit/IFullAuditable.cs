using AmericanVirtual.Weather.Challenge.Database.Model;

namespace AmericanVirtual.Weather.Challenge.Database.Audit
{
    public interface IFullAuditable
    {
        DateTime? CreationDate { get; set; }
        User CreatedBy { get; set; }
        DateTime? ModificationDate { get; set; }
        User ModifiedBy { get; set; }
    }
}