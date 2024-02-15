using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmericanVirtual.Weather.Challenge.Database.Model
{
    [Table("UserLogins")]
    public class UserLogin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        public virtual User User { get; set; }
        public long UserID { get; set; }
        public DateTime CreationDate { get; set; }

        public UserLogin()
        {
            CreationDate = DateTime.Now;
        }
    }
}