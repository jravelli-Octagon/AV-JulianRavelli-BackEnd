using AmericanVirtual.Weather.Challenge.Common.DTOs;
using AmericanVirtual.Weather.Challenge.Common.Helper;
using AmericanVirtual.Weather.Challenge.Database.Audit;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmericanVirtual.Weather.Challenge.Database.Model
{
    [Table("UserTickets")]
    public class UserTicket : PartialAuditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        public long UserID { get; set; }
        public virtual User User { get; set; }
        public string TicketType { get; set; }
        public string Hash { get; set; }
        public bool IsActive { get; set; }

        public UserTicket()
        {
        }

        public UserTicket(User user, string username, string ticketType)
        {
            IsActive = true;
            Hash = CryptoHelper.Encrypt256(JsonConvert.SerializeObject(new HashDTO(username)));
            TicketType = ticketType;
            User = user;
        }

        public void Desactive()
        {
            IsActive = false;
        }
    }
}