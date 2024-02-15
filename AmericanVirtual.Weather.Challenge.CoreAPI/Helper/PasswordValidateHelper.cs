using AmericanVirtual.Weather.Challenge.Database.Model;

namespace AmericanVirtual.Weather.Challenge.CoreAPI.Helper
{
    public static class PasswordValidateHelper
    {
        public static bool Validate(PasswordPolicy policies, string password)
        {
            _ = policies ?? throw new ArgumentNullException(nameof(policies));

            if (string.IsNullOrEmpty(password))
            {
                return false;
            }

            if (password.Length <= policies.MinimumLength)
            {
                return false;
            }

            if (password.Length >= policies.MaximumLength)
            {
                return false;
            }

            var lowerCount = password.Count(c => char.IsLower(c));

            if (policies.MinimumNumberOfLowercaseLetters >= lowerCount)
            {
                return false;
            }

            var upperCount = password.Count(c => char.IsUpper(c));

            if (policies.MinimumNumberOfUppercaseLetters > upperCount)
            {
                return false;
            }

            var numberOfSpecialCharacters = password.Count(c => !char.IsLetterOrDigit(c));

            if (policies.MinimumNumberOfSpecialCharacters > numberOfSpecialCharacters)
            {
                return false;
            }

            return true;
        }
    }
}