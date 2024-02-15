using AmericanVirtual.Weather.Challenge.Common.Enums;
using AmericanVirtual.Weather.Challenge.Common.Helper;
using AmericanVirtual.Weather.Challenge.Database.Audit;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmericanVirtual.Weather.Challenge.Database.Model
{

    [Table("Users")]
    public class User : FullAuditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int FailedAttempts { get; set; }
        [Column(TypeName = "Date")]
        public DateTime PasswordChangeDate { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public virtual ICollection<UserLogin> Logings { get; set; }
        public virtual ICollection<UserTicket> Tickets { get; set; }
        public string CurrentState { get; set; } = string.Empty;
        public virtual ICollection<UserStateHistory> UserStatesHistory { get; set; }
        public virtual ICollection<AuthenticationToken> AuthenticationTokens { get; set; }

        public void IncrementFailedAttempts()
        {
            FailedAttempts += 1;
        }

        public void ChangeUserState(string newState)
        {
            if (eUserStates.DELETED.ToString().Equals(newState))
            {
                IsActive = false;
            }
            else if (eUserStates.ACTIVE.ToString().Equals(newState))
            {
                IsActive = true;
            }

            CurrentState = newState;

            UserStateHistory history = new UserStateHistory(this, newState);

            if (UserStatesHistory == null)
            {
                UserStatesHistory = new List<UserStateHistory>();
            }

            UserStatesHistory.Add(history);
        }

        public void Login()
        {
            FailedAttempts = 0;

            if (Logings == null)
            {
                Logings = new List<UserLogin>();
            }

            Logings.Add(new UserLogin());
        }

        public string CreateTicket(string ticketType)
        {
            UserTicket ticket = new UserTicket(this, Username, ticketType);

            if (Tickets == null)
            {
                Tickets = new List<UserTicket>();
            }

            Tickets.Add(ticket);

            return ticket.Hash;
        }

        public void CreateAuthenticationToken(string token)
        {
            AuthenticationToken authenticationToken = new AuthenticationToken(this, token);

            if (AuthenticationTokens == null)
            {
                AuthenticationTokens = new List<AuthenticationToken>();
            }

            AuthenticationTokens.Add(authenticationToken);
        }
        public void ChangePassword(string newPassword)
        {
            Password = CryptoHelper.HashPasswordInternal(newPassword);
            PasswordChangeDate = DateTime.Now;
            FailedAttempts = 0;
        }
    }
}