namespace Microservices.Common
{
    public static class EncryptionExtensions
    {
        public static string Encrypt(this string stringToEncrypt)
        {
            return Encryption.Encrypt(stringToEncrypt);
        }

        public static string Decrypt(this string stringToDecrypt)
        {
            return Encryption.Decrypt(stringToDecrypt);
        }
    }
}