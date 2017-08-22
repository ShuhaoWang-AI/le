using System;
using System.IO;
using System.Security.Cryptography;

namespace Linko.LinkoExchange.Services.CopyOfRecord
{
    public class HashHelper
    {
        public static string CalculateFileHash(string fileName)
        {
            var data = File.ReadAllBytes(path:fileName);
            return ComputeSha256Hash(data:data);
        }

        public static string ComputeSha256Hash(byte[] data)
        {
            using (var sha256 = new SHA256Managed())
            {
                var hash = sha256.ComputeHash(buffer:data);
                var hex = ByteArrayToString(data:hash).ToLowerInvariant();
                return hex;
            }
        }

        private static string ByteArrayToString(byte[] data)
        {
            var hex = BitConverter.ToString(value:data);
            return hex.Replace(oldValue:"-", newValue:"");
        }
    }
}