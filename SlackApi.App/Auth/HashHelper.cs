using System.Security.Cryptography;
using System.Text;

namespace SlackApi.App.Auth
{
    public static class HashHelper
    {
        public static string GetSHA256HashFromKey(string key, string input)
            => GetHash(new HMACSHA256(Encoding.UTF8.GetBytes(key)), input);

        public static string GetHash(HashAlgorithm hashAlgorithm, string input)
            => BitConverter.ToString(hashAlgorithm
                .ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", "")
                .ToLower();

        public static string GetHash(HashAlgorithm hashAlgorithm, byte[] input)
            => BitConverter.ToString(hashAlgorithm
                .ComputeHash(input)).Replace("-", "")
                .ToLower();

        // Verify a hash against a string.
        public static bool VerifyHash(HashAlgorithm hashAlgorithm, string input, string hash)
        {
            // Hash the input.
            var hashOfInput = GetHash(hashAlgorithm, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return comparer.Compare(hashOfInput, hash) == 0;
        }
    }
}
