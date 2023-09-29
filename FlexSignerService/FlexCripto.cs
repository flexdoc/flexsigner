using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FlexSignerService
{
    public class FlexCripto
    {
        private const int IV_SIZE = 16;
        private static byte[] aes256SecretKey;

        public byte[] FileEncrypt(byte[] byteArrayToEncrypt, byte[] passwordBytes, byte[] iv)
        {
            try
            {
                byte[] initVector = iv;

                if (initVector == null )
                    initVector = new byte[IV_SIZE];

                using (var aes = new AesCryptoServiceProvider())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = passwordBytes; 
                    aes.IV = initVector;

                    using (var encryptor = aes.CreateEncryptor())
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(byteArrayToEncrypt, 0, byteArrayToEncrypt.Length);
                            csEncrypt.FlushFinalBlock();
                        }

                        byte[] encrypted = msEncrypt.ToArray();
                        byte[] encryptionTag = new byte[] { }; 
                        byte[] encryptedFinal = new byte[encryptionTag.Length + initVector.Length + encrypted.Length];

                        Array.Copy(encrypted, 0, encryptedFinal, encryptionTag.Length, encrypted.Length);

                        return encryptedFinal;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                throw ex;
            }
        }

        public byte[] FileDecrypt(byte[] byteArrayToDecrypt, byte[] passwordBytes, byte[] IV)
        {
            try
            {
                byte[] encryptionTag = (new byte[] { });
                byte[] initVector = new byte[IV_SIZE];

                Array.Copy(byteArrayToDecrypt, byteArrayToDecrypt.Length - IV_SIZE, initVector, 0, IV_SIZE);

                using (var aes = new AesCryptoServiceProvider())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.None;
                    aes.Key = passwordBytes; 
                    aes.IV = initVector;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var msDecrypt = new MemoryStream(byteArrayToDecrypt))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        byte[] decrypted = new byte[byteArrayToDecrypt.Length - encryptionTag.Length - IV_SIZE];
                        int decryptedByteCount = csDecrypt.Read(decrypted, 0, decrypted.Length);

                        return decrypted;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                throw ex;
            }

        }


        public byte[] GetAes256SecretKey( string password = "A4TyX3KUXrqK8weoUtQxJHrWEXPDmsTG")
        {
            try
            {
                aes256SecretKey = Encoding.UTF8.GetBytes(password);

                return aes256SecretKey;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                throw e;
            }
        }

    }
}
