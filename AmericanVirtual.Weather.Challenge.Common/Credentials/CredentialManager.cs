using CredentialManagement;

namespace AmericanVirtual.Weather.Challenge.Common.Credentials
{
    public static class CredentialManager
    {
        public static string GetConnectionString(string username, string password, string connectionString)
        {
            return connectionString.Replace("[DB-USERID]", username).Replace("[DB-PASSWORD]", password);
        }

        public static string GetSecretString(string target)
        {
            using var credential = new Credential
            {
                Target = target
            };

            credential.Load();
            return credential.Password;
        }

        public static Credential GetCredential(string target)
        {
            using var credential = new Credential
            {
                Target = target
            };

            credential.Load();
            
            return new Credential
            {
                Username = credential.Username,
                Password = credential.Password
            };
        }

        public static string[] GetSecretStrings(string target)
        {
            using var credential = new Credential
            {
                Target = target
            };
            credential.Load();
            return new string[2] { credential.Username, credential.Password };
        }

    }
}