using System;
using System.IO;
using System.Security.Cryptography;

namespace Linko.LinkoExchange.Services.CopyOfRecord
{
    public class HashHelper
    {
        public static string CalculateFileHash(string fileName)
        {
            var data = File.ReadAllBytes(fileName);
            return ComputeSha256Hash(data);
        }

        public static string ComputeSha256Hash(byte[] data)
        {
            using (var sha256 = new SHA256Managed())
            {
                var hash = sha256.ComputeHash(data);
                var hex = ByteArrayToString(hash).ToLowerInvariant();
                return hex;
            }
        }

        private static string ByteArrayToString(byte[] data)
        {
            string hex = BitConverter.ToString(data);
            return hex.Replace(oldValue: "-", newValue: "");
        }
    }
}
