using AmericanVirtual.Weather.Challenge.Database.Audit;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmericanVirtual.Weather.Challenge.Database.Model
{
    [Table("UserStateHistories")]
    public class UserStateHistory : PartialAuditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        public virtual User User { get; set; }
        public long UserID { get; set; }
        public string UserState { get; set; }

        public UserStateHistory()
        {
        }

        public UserStateHistory(User user, string userState)
        {
            UserState = userState;
            User = user;
        }
    }
}