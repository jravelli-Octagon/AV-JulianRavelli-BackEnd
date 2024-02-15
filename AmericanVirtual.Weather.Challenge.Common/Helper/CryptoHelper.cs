using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

namespace AmericanVirtual.Weather.Challenge.Common.Helper
{
    public static class CryptoHelper
    {
        private const int IterationCount = 10000;
        private const int SubKeyLength = 256 / 8; // 256 bits
        private const int SaltSize = 128 / 8; // 128 bits

        private static readonly RandomNumberGenerator _rand = RandomNumberGenerator.Create();

        private const string AesIV256 = @"!QAZ2WSX#EDC4RFV";
        private const string AesKey256 = @"5TGB&YHN7UJM(IK<5TGB&YHN7UJM(IK<";

        public static string HashPasswordInternal(string password, KeyDerivationPrf prf = KeyDerivationPrf.HMACSHA256,
            int iterationCount = IterationCount, int saltSize = SaltSize, int numBytesRequested = SubKeyLength)
        {
            byte[] salt = new byte[saltSize];
            _rand.GetBytes(salt);
            byte[] subKey = KeyDerivation.Pbkdf2(password, salt, prf, iterationCount, numBytesRequested);

            byte[] outputBytes = new byte[13 + salt.Length + subKey.Length];
            outputBytes[0] = 0x01; // format marker
            WriteNetworkByteOrder(outputBytes, 1, (uint)prf);
            WriteNetworkByteOrder(outputBytes, 5, (uint)iterationCount);
            WriteNetworkByteOrder(outputBytes, 9, (uint)saltSize);
            Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
            Buffer.BlockCopy(subKey, 0, outputBytes, 13 + saltSize, subKey.Length);

            return Convert.ToBase64String(outputBytes);
        }

        public static bool VerifyHashedPasswordInternal(byte[] hashedPassword, string providedPassword)
        {
            try
            {
                _ = hashedPassword ?? throw new ArgumentNullException(nameof(hashedPassword));

                // Read header information
                var prf = (KeyDerivationPrf)ReadNetworkByteOrder(hashedPassword, 1);
                int iterationCount = (int)ReadNetworkByteOrder(hashedPassword, 5);
                var saltLength = (int)ReadNetworkByteOrder(hashedPassword, 9);

                // Read the salt: must be >= 128 bits
                if (saltLength < 128 / 8)
                {
                    return false;
                }

                var salt = new byte[saltLength];
                Buffer.BlockCopy(hashedPassword, 13, salt, 0, salt.Length);

                // Read the subKey (the rest of the payload): must be >= 128 bits
                int subKeyLength = hashedPassword.Length - 13 - salt.Length;
                if (subKeyLength < 128 / 8)
                {
                    return false;
                }

                var expectedSubKey = new byte[subKeyLength];
                Buffer.BlockCopy(hashedPassword, 13 + salt.Length, expectedSubKey, 0, expectedSubKey.Length);

                // Hash the incoming password and verify it
                var actualSubKey = KeyDerivation.Pbkdf2(providedPassword, salt, prf, iterationCount, subKeyLength);
                return ByteArraysEqual(actualSubKey, expectedSubKey);
            }
            catch (Exception)
            {
                // This should never occur except in the case of a malformed payload, where
                // we might go off the end of the array. Regardless, a malformed payload
                // implies verification failed.
                throw;
            }
        }
        
        public static string Encrypt256(string text)
        {
            // AesCryptoServiceProvider
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;
                aes.IV = Encoding.UTF8.GetBytes(AesIV256);
                aes.Key = Encoding.UTF8.GetBytes(AesKey256);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Convert string to byte array
                byte[] src = Encoding.UTF8.GetBytes(text);

                // encryption
                using (ICryptoTransform encrypt = aes.CreateEncryptor())
                {
                    byte[] dest = encrypt.TransformFinalBlock(src, 0, src.Length);

                    // Convert byte array to Base64 strings
                    return Convert.ToBase64String(dest);
                }
            }
        }

        public static string Decrypt256(string text)
        {
            // AesCryptoServiceProvider
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;
                aes.IV = Encoding.UTF8.GetBytes(AesIV256);
                aes.Key = Encoding.UTF8.GetBytes(AesKey256);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Convert Base64 strings to byte array
                byte[] src = System.Convert.FromBase64String(text);

                // decryption
                using (ICryptoTransform decrypt = aes.CreateDecryptor())
                {
                    byte[] dest = decrypt.TransformFinalBlock(src, 0, src.Length);
                    return Encoding.UTF8.GetString(dest);
                }
            }
        }

        #region private methods
        private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
        {
            return ((uint)(buffer[offset + 0]) << 24)
                   | ((uint)(buffer[offset + 1]) << 16)
                   | ((uint)(buffer[offset + 2]) << 8)
                   | buffer[offset + 3];
        }

        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (a == null && b == null)
            {
                return true;
            }

            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            var areSame = true;
            for (var i = 0; i < a.Length; i++)
            {
                areSame &= (a[i] == b[i]);
            }

            return areSame;
        }

        private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
        {
            buffer[offset + 0] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)(value >> 0);
        }
        #endregion

    }
}