using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Collections;
using System.Windows;
using System.Text.RegularExpressions;

namespace KMASafeKidCore
{
    internal class CRSA
    {
        public static Dictionary<string,string> GenerateKey()
        {
            Dictionary<string, string> dictKey = new Dictionary<string, string>();
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            string publicKey = rsa.ToXmlString(false);
            string privateKey = rsa.ToXmlString(true);
            dictKey.Add("PUB", publicKey);
            dictKey.Add("PRI", privateKey);
            return dictKey;
        }
        
        // Import PEM Key include padding
        public static string Encrypt(string text, string publicKeyPem)
        {
            string stringEncrypt;
            try
            {
                using (var rsa = RSAKeys.ImportPublicKey(Regex.Unescape(publicKeyPem)))
                {
                    //rsa.FromXmlString(publicKey);
                    
                    var data = Encoding.UTF8.GetBytes(text);
                    var cypher = rsa.Encrypt(data, true);
                    stringEncrypt = Convert.ToBase64String(cypher);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception encrypt:" + e.Message);
                return null;
            }
            return stringEncrypt;
        }
        public static string Decrypt(string cypher, string privateKey)
        {
            string stringDecrypt;
            try
            {
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(privateKey);
                    var data = Convert.FromBase64String(cypher);
                    var text = rsa.Decrypt(data, true);
                    stringDecrypt = Encoding.UTF8.GetString(text);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception decrypt:" + e.Message);
                return null;
            }
            return stringDecrypt ;
        }

        public static byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {

                    //Import the RSA Key information. This only needs
                    //toinclude the public key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Encrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                }
                return encryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }

        public static byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] decryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Import the RSA Key information. This needs
                    //to include the private key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Decrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
                }
                return decryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }
        }
    }
}
