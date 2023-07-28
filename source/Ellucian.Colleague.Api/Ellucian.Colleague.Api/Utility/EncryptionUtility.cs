using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Ellucian.Colleague.Api.Utility
{
    /// <summary>
    /// 
    /// </summary>
    public static class EncryptionUtility
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plainText">Text to encrypt.</param>
        /// <returns></returns>
        public static string Encrypt(string plainText)
        {
            var plaintextBytes = Encoding.UTF8.GetBytes(plainText);
            try
            {
                return MachineKey.Encode(plaintextBytes, MachineKeyProtection.All);
            }
            catch
            {
                return plainText;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encrypted">Encrypted text to decrypt.</param>
        /// <returns></returns>
        public static string Decrypt(string encrypted)
        {
            try
            {
                var decryptedBytes = MachineKey.Decode(encrypted, MachineKeyProtection.All);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch
            {
                return encrypted;
            }
        }

        /// <summary>
        /// Encrypts a string using the MachineKey API
        /// then converts to base 64 encoding
        /// </summary>
        /// <param name="text">String to protect</param>
        /// <returns>protected string</returns>
        public static string Protect(string text)
        {
            var protectedBytes = MachineKey.Protect(Encoding.UTF8.GetBytes(text));
            return Convert.ToBase64String(protectedBytes);
        }

        /// <summary>
        /// Decrypts a protected string using the MachineKeyAPI
        /// then converts from base 64 encoding
        /// </summary>
        /// <param name="base64EncodedProtected">Base 64 encoded protected string</param>
        /// <returns>unprotected string</returns>
        public static string Unprotect(string base64EncodedProtected)
        {
            var protectedBytes = Convert.FromBase64String(base64EncodedProtected);
            var unprotectedBytes = MachineKey.Unprotect(protectedBytes);
            return Encoding.UTF8.GetString(unprotectedBytes);
        }
    }
}