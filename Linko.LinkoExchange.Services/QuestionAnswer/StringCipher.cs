using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Linko.LinkoExchange.Services.QuestionAnswer
{
    public static class StringCipher
    {
        #region static fields and constants

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        private const string EncryptionKey = "uLFaCvjDA.%*-Jbune;h/^[#UDv-.xABvk4q;";

        // This constant is used to determine the key size of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        #endregion

        public static string Encrypt(string plainText)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(s:plainText);
            using (var password = new Rfc2898DeriveBytes(password:EncryptionKey, salt:saltStringBytes, iterations:DerivationIterations))
            {
                var keyBytes = password.GetBytes(cb:Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(rgbKey:keyBytes, rgbIV:ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(stream:memoryStream, transform:encryptor, mode:CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(buffer:plainTextBytes, offset:0, count:plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();

                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(second:ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(second:memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(inArray:cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(s:cipherText);

            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(count:Keysize / 8).ToArray();

            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(count:Keysize / 8).Take(count:Keysize / 8).ToArray();

            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip(count:Keysize / 8 * 2).Take(count:cipherTextBytesWithSaltAndIv.Length - Keysize / 8 * 2).ToArray();

            using (var password = new Rfc2898DeriveBytes(password:EncryptionKey, salt:saltStringBytes, iterations:DerivationIterations))
            {
                var keyBytes = password.GetBytes(cb:Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(rgbKey:keyBytes, rgbIV:ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(buffer:cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(stream:memoryStream, transform:decryptor, mode:CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(buffer:plainTextBytes, offset:0, count:plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(bytes:plainTextBytes, index:0, count:decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(data:randomBytes);
            }
            return randomBytes;
        }
    }
}