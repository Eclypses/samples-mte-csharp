using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MteConsoleMultipleClients.Helpers
{
    public class AesHelper
    {
        #region Encrypt
        /// <summary>
        /// Encrypts the specified plain text.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <param name="key">The key.</param>
        /// <param name="IV">The iv.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="ApplicationException">Item cannot be blank or null</exception>
        /// <exception cref="ApplicationException">Encryption key cannot be blank or null</exception>
        /// <exception cref="ApplicationException">Encryption IV cannot be blank or null</exception>
        public string Encrypt(string plainText, string key, string IV)
        {
            string encryptedText = "";
            try
            {
                if (string.IsNullOrWhiteSpace(plainText))
                {
                    throw new ApplicationException("Item cannot be blank or null");
                }

                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new ApplicationException("Encryption key cannot be blank or null");
                }

                if (string.IsNullOrWhiteSpace(IV))
                {
                    throw new ApplicationException("Encryption IV cannot be blank or null");
                }
                byte[] fuzzyBytes;
                using (var sha = SHA256.Create())
                {
                    //
                    // Get the hash of the crypto key
                    //
                    byte[] keyBytes = sha.ComputeHash(ASCIIEncoding.ASCII.GetBytes(key));

                    //
                    // Get the byte array from the IV
                    //
                    byte[] ivBytes = ASCIIEncoding.ASCII.GetBytes(IV.ToString());
                    using (var aes = Aes.Create())
                    {

                        using (ICryptoTransform encryptor = aes.CreateEncryptor(keyBytes, ivBytes.Take(16).ToArray()))
                        {
                            using (var ms = new MemoryStream())
                            {
                                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                                {
                                    using (var sw = new StreamWriter(cs))
                                    {
                                        sw.Write(plainText);
                                    }
                                    fuzzyBytes = ms.ToArray();
                                }
                            }
                        }
                    }
                }
                encryptedText = Convert.ToBase64String(fuzzyBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            return encryptedText;

        }
        #endregion

        #region Decrypt
        /// <summary>
        /// Decrypts the specified encrypted.
        /// </summary>
        /// <param name="encrypted">The encrypted.</param>
        /// <param name="key">The key.</param>
        /// <param name="IV">The iv.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="ApplicationException">Item cannot be blank or null</exception>
        /// <exception cref="ApplicationException">Encryption key cannot be blank or null</exception>
        /// <exception cref="ApplicationException">Encryption IV cannot be blank or null</exception>
        public string Decrypt(string encrypted, string key, string IV)
        {
            string clearText = String.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(encrypted))
                {
                    throw new ApplicationException("Item cannot be blank or null");
                }

                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new ApplicationException("Encryption key cannot be blank or null");
                }

                if (string.IsNullOrWhiteSpace(IV))
                {
                    throw new ApplicationException("Encryption IV cannot be blank or null");
                }

                using (var sha = SHA256.Create())
                {
                    //
                    // Convert the Base64 string into a byte array
                    //
                    byte[] fuzzy = Convert.FromBase64String(encrypted);
                    //
                    // Then get the hash of the crypto key
                    //
                    byte[] keyBytes = sha.ComputeHash(ASCIIEncoding.ASCII.GetBytes(key));
                    //
                    // Get the byte array from the IV
                    //
                    byte[] ivBytes = ASCIIEncoding.ASCII.GetBytes(IV.ToString());
                    //
                    // Decrypt the data into a string.
                    //
                    using (var aes = Aes.Create())
                    {
                        using (ICryptoTransform decryptor = aes.CreateDecryptor(keyBytes, ivBytes.Take(16).ToArray()))
                        {
                            using (var ms = new MemoryStream(fuzzy))
                            {
                                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                                {
                                    using (var sr = new StreamReader(cs))
                                    {
                                        clearText = sr.ReadToEnd();
                                    }
                                }
                            }
                        }
                    }
                    return clearText;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
        #endregion
    }
}
