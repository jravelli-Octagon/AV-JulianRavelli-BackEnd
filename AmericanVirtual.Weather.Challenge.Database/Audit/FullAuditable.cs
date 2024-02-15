using AmericanVirtual.Weather.Challenge.Database.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmericanVirtual.Weather.Challenge.Database.Audit
{
    public class FullAuditable : IFullAuditable
    {
        public DateTime? CreationDate { get; set; }

        [ForeignKey("CreatedByID")]
        public User CreatedBy { get; set; }

        public long? CreatedByID { get; set; }
        public DateTime? ModificationDate { get; set; }

        [ForeignKey("ModifiedByID")]
        public User ModifiedBy { get; set; }
        public long? ModifiedByID { get; set; }
    }
}