using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmericanVirtual.Weather.Challenge.Database.Model
{
    //! Política de contraseña
    /*!
    */
    [Table("PasswordPolicy")]
    public class PasswordPolicy
    {
        [Key]
        public long ID { get; set; }
        public int MinimumLength { get; set; }
        public int MaximumLength { get; set; }
        public int NumberOfDaysUntilExpire { get; set; }
        public int NumberOfFailedAttemptsAllowed { get; set; }
        public int MinimumNumberOfLowercaseLetters { get; set; }
        public int MinimumNumberOfUppercaseLetters { get; set; }
        public int MinimumNumberOfSpecialCharacters { get; set; }
    }
}