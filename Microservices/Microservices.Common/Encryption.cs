using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Microservices.Common
{
    [ExcludeFromCodeCoverage]
    public static class Encryption
    {
        // Encryption Key
        private const string Key = "Complete_Overhaulin!";

        // Set your salt here, but it must be at least 8 bytes/ characters.
        private static readonly byte[] SaltBytes = { 10, 07, 04, 08, 12, 06, 02, 14, 13, 01, 16, 05, 09, 15, 03, 11 };

        public static string Encrypt(string stringToBeEncrypted)
        {
            return Convert.ToBase64String(GetEncryptedOrDecryptedByteArray(stringToBeEncrypted));
        }

        public static string Decrypt(string stringToBeDecrypted)
        {
            if (!stringToBeDecrypted.ToLowerInvariant().Contains("password") ||
                !stringToBeDecrypted.ToLowerInvariant().Contains("user id"))
                return Encoding.UTF8.GetString(GetEncryptedOrDecryptedByteArray(stringToBeDecrypted, false));

            var stringArray = stringToBeDecrypted.Split(';');

            var encryptedUserId = Array
                .Find(stringArray, p => p.ToLowerInvariant().StartsWith("user id", StringComparison.Ordinal))
                .Substring(8);
            var encryptedPassword =
                Array.Find(stringArray, p => p.ToLowerInvariant().StartsWith("password", StringComparison.Ordinal))
                    .Substring(9);

            var decryptedUserId = Encoding.UTF8.GetString(GetEncryptedOrDecryptedByteArray(encryptedUserId, false));
            var decryptedPassword = Encoding.UTF8.GetString(GetEncryptedOrDecryptedByteArray(encryptedPassword, false));

            return stringToBeDecrypted.Replace(encryptedUserId, decryptedUserId)
                .Replace(encryptedPassword, decryptedPassword);
        }

        private static byte[] HashKey()
        {
            var keyBytes = Encoding.UTF8.GetBytes(Key);

            // Hash the key with SHA256
            keyBytes = SHA256.Create().ComputeHash(keyBytes);

            // Return byte array
            return keyBytes;
        }

        private static byte[] GetEncryptedOrDecryptedByteArray(string inputString, bool isEncrypt = true)
        {
            byte[] bytes;

            using (var ms = new MemoryStream())
            {
                using (var aes = new RijndaelManaged())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(HashKey(), SaltBytes, 1000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);

                    aes.Mode = CipherMode.CBC;

                    using (
                        var cs = new CryptoStream(ms, isEncrypt ? aes.CreateEncryptor() : aes.CreateDecryptor(),
                            CryptoStreamMode.Write))
                    {
                        var byteArray = isEncrypt
                            ? Encoding.UTF8.GetBytes(inputString)
                            : Convert.FromBase64String(inputString);

                        cs.Write(byteArray, 0, byteArray.Length);
                        cs.Close();
                    }

                    bytes = ms.ToArray();
                }
            }

            return bytes;
        }
    }
}