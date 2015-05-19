using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace _console {
    class Program {
        static void Main(string[] args) {

            string plainText = "";    // original plaintext
            string cipherText = "";                 // encrypted text
            string passPhrase = "";        // can be any string
            //string initVector = ""; // must be 16 bytes



            // Before encrypting data, we will append plain text to a random
            // salt value, which will be between 4 and 8 bytes long (implicitly
            // used defaults).

            Console.WriteLine(String.Format("Plaintext   : {0}\n", plainText));

            // Encrypt the same plain text data 10 time (using the same key,
            // initialization vector, etc) and see the resulting cipher text;
            // encrypted values will be different.
            for (int i = 0; i < 10; i++) {
                var initVector = tgsdesktop.utilities.RijndaelEnhanced.GenerateRandomString(16);
                var rijndaelKey =
                    new tgsdesktop.utilities.RijndaelEnhanced(passPhrase, initVector);
                cipherText = rijndaelKey.Encrypt(plainText);
                cipherText = initVector + cipherText;
                System.Diagnostics.Debug.WriteLine(
                    String.Format("Encrypted #{0}: {1}", i, cipherText));

                rijndaelKey = new tgsdesktop.utilities.RijndaelEnhanced(passPhrase, initVector);
                plainText = rijndaelKey.Decrypt(cipherText.Substring(16, cipherText.Length - 16));
            }
            Console.ReadLine();

            // Make sure we got decryption working correctly.
            Console.WriteLine(String.Format("\nDecrypted   : {0}", plainText));
        }

        static string GetUniqueKey(int maxSize) {
            char[] chars = new char[62];
            chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider()) {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data) {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }
    }
}
