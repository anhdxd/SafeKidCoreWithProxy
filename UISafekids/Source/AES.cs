using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UISafekids.Source
{
    class AES
    {
        private static readonly string sMyKey = "35bf1ca462f3b760510ffa1dfc2e173c";// MD5 Pass
        private static readonly string sVectorIV = "chiukhongbietxxx";// Vecto 16byte
        public static void TestAes()
        {
            using (Aes myAes = Aes.Create())
            {
                UnicodeEncoding UE = new UnicodeEncoding();
                myAes.Key = Encoding.UTF8.GetBytes(sMyKey);
                myAes.IV = Encoding.UTF8.GetBytes(sVectorIV);
                //myAes.Mode = CipherMode.CTS;
                //myAes.Padding = PaddingMode.PKCS7;

                // Encrypt the string to an array of bytes.
                //byte[] block = File.ReadAllBytes(@".\DomainAdult.dat");
                string original = File.ReadAllText(@".\GameBlock.dat");
                // string original = System.Text.Encoding.UTF8.GetString(block);

                byte[] encrypted = EncryptStringToBytes_Aes(original, myAes.Key, myAes.IV);

                File.WriteAllBytes(@".\encrypt.dat", encrypted);

                // Decrypt the bytes to a string.
                byte[] getbyte = File.ReadAllBytes(@".\encrypt.dat");
                string roundtrip = DecryptStringFromBytes_Aes(getbyte, myAes.Key, myAes.IV);

                var listAdult = new SortedSet<string>();
                string[] sParam = { "\r\n" };
                string[] Split = roundtrip.Split(sParam, StringSplitOptions.None);
                listAdult.Contains("Host");
                //Display the original data and the decrypted data.
                //Console.WriteLine("Original: \r\n" + original);
                Console.WriteLine("Round Trip: \r\n" + roundtrip);
            }
        }
        public static bool EncryptSortedSetToFile(SortedSet<string> listSort, string sDisPath)
        {
            try
            {
                using (Aes myAes = Aes.Create())
                {
                    myAes.Key = Encoding.UTF8.GetBytes(sMyKey);
                    myAes.IV = Encoding.UTF8.GetBytes(sVectorIV);
                    string original = "";

                    foreach (var item in listSort)
                    {
                        original += item + "\r\n";
                    }

                    byte[] encrypted = EncryptStringToBytes_Aes(original, myAes.Key, myAes.IV);

                    File.WriteAllBytes(sDisPath, encrypted);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error EncryptSortedSetToFile {0} with error: {1} ", sDisPath, e.Message);
                return false;
            }
            return true;
        }
        public static bool EncryptFileToFile(string sSourPath, string sDisPath, string AddName)
        {
            try
            {
                using (Aes myAes = Aes.Create())
                {
                    myAes.Key = Encoding.UTF8.GetBytes(sMyKey);
                    myAes.IV = Encoding.UTF8.GetBytes(sVectorIV);

                    string original = File.ReadAllText(sSourPath);
                    original += "\r\n" + AddName;
                    // string original = System.Text.Encoding.UTF8.GetString(block);

                    byte[] encrypted = EncryptStringToBytes_Aes(original, myAes.Key, myAes.IV);

                    File.WriteAllBytes(sDisPath, encrypted);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error EncryptFileToFile {0} with error: {1} ", sSourPath, e.Message);
                return false;
            }
            return true;
        }
        public static bool EncryptFileToFile(string sSourPath, string sDisPath)
        {
            try
            {
                using (Aes myAes = Aes.Create())
                {
                    myAes.Key = Encoding.UTF8.GetBytes(sMyKey);
                    myAes.IV = Encoding.UTF8.GetBytes(sVectorIV);

                    string original = File.ReadAllText(sSourPath);
                    // string original = System.Text.Encoding.UTF8.GetString(block);

                    byte[] encrypted = EncryptStringToBytes_Aes(original, myAes.Key, myAes.IV);

                    File.WriteAllBytes(sDisPath, encrypted);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error EncryptFileToFile {0} with error: {1} ", sSourPath, e.Message);
                return false;
            }
            return true;
        }
        public static SortedSet<string> DecryptFileToSortedSet(string sPath)
        {
            SortedSet<string> sortList = new SortedSet<string>();
            string[] parSub = { "\r\n" };
            try
            {


                using (Aes myAes = Aes.Create())
                {
                    UnicodeEncoding UE = new UnicodeEncoding();
                    myAes.Key = Encoding.UTF8.GetBytes(sMyKey);
                    myAes.IV = Encoding.UTF8.GetBytes(sVectorIV);
                    //myAes.Mode = CipherMode.CBC;
                    //myAes.Padding = PaddingMode.PKCS7;

                    byte[] getbyte = File.ReadAllBytes(sPath);
                    string roundtrip = DecryptStringFromBytes_Aes(getbyte, myAes.Key, myAes.IV);

                    string[] sSplit = roundtrip.Split(parSub, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in sSplit)
                    {
                        sortList.Add(item);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error DecryptFileToSortedSet " + sPath + " With Err:" + e.Message);
            }
            return sortList;
        }
        public static string EncryptStringToBase64(string sEncrypt)
        {
            string base64String;
            try
            {
                using (Aes myAes = Aes.Create())
                {
                    myAes.Key = Encoding.UTF8.GetBytes(sMyKey);
                    myAes.IV = Encoding.UTF8.GetBytes(sVectorIV);

                    // string original = System.Text.Encoding.UTF8.GetString(block);
                    byte[] encrypted = EncryptStringToBytes_Aes(sEncrypt, myAes.Key, myAes.IV);
                    base64String = Convert.ToBase64String(encrypted);
                    //sEncrypt = System.Text.Encoding.UTF8.GetString(encrypted);
                }
            }
            catch (Exception)
            {
                return null;
            }
            return base64String;
        }
        public static string DecryptBase64ToString(string sDecrypt)
        {
            string original;
            try
            {
                using (Aes myAes = Aes.Create())
                {
                    myAes.Key = Encoding.UTF8.GetBytes(sMyKey);
                    myAes.IV = Encoding.UTF8.GetBytes(sVectorIV);

                    byte[] encrypted = Convert.FromBase64String(sDecrypt);

                    // string original = System.Text.Encoding.UTF8.GetString(block);
                    original = DecryptStringFromBytes_Aes(encrypted, myAes.Key, myAes.IV);
                    //sDecrypt = original;
                    //sEncrypt = System.Text.Encoding.UTF8.GetString(encrypted);
                }
            }
            catch (Exception)
            {
                return null;
            }
            return original;
        }
        private static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        private static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}
