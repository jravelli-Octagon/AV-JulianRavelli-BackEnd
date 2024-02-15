using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AmericanVirtual.Weather.Challenge.Common.Extensions
{
    public static class StringExtensions
    {
        public static string PackToString(this IEnumerable<string> values, string packingSymbol)
        {
            return string.Join(packingSymbol, values);
        }
        public static IEnumerable<string> UnpackFromString(this string packedValues, string packingSymbol)
        {
            if (packedValues == null) throw new ArgumentNullException(nameof(packedValues));

            return packedValues.Split(new[] { packingSymbol }, StringSplitOptions.None);
        }

        public static string GetRandomString(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder randomString = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    randomString.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }

            return randomString.ToString();
        }

        public static IEnumerable<string> SplitStringToLines(this string input)
        {
            if (input == null)
            {
                yield break;
            }

            using StringReader reader = new StringReader(input);

            string line;

            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }
}