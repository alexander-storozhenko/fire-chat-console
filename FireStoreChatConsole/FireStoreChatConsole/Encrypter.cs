using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FireStoreChatConsole
{
    class Encrypter
    {

        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
        private RSAParameters privateKey;
        private RSAParameters publicKey;
        public string publicKeyOut="";

        public Encrypter()
        {
            privateKey = rsa.ExportParameters(true);
            publicKey = rsa.ExportParameters(false);
        }

        public string PublicKeyString()
        {
            var sw = new StringWriter();
            var xs = new XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, publicKey);
            return sw.ToString();
        }

        public string Encrypt(string str)
        {
            publicKeyOut = string.IsNullOrEmpty(publicKeyOut) ? Settings.db.GetOutKey(User.login): publicKeyOut;
            //Console.WriteLine(publicKeyOut);
            rsa = new RSACryptoServiceProvider(2048);
            rsa.FromXmlString(publicKeyOut);
            var data = Encoding.UTF8.GetBytes(str);
            Console.WriteLine(str.Length);
            return Convert.ToBase64String(rsa.Encrypt(data, false));
        }

        public string Decrypt(string str)
        {
            rsa.ImportParameters(privateKey);

            var text = rsa.Decrypt(Convert.FromBase64String(str), false);

            return Encoding.UTF8.GetString(text);
        }

        //public byte[] Encrypt(string str)
        //{
        //    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        //    rsa.FromXmlString(key);
        //    return rsa.Encrypt(GetBytes(str), false);
        //}

        //public string Decrypt(byte[] str)
        //{
        //    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        //    rsa.FromXmlString(key);
        //    return GetString(rsa.Decrypt(str, false));
        //}

        //private byte[] GetBytes(string str)
        //{
        //    return Encoding.UTF8.GetBytes(str);
        //}
        //public string GetString(byte[] str)
        //{
        //    return Encoding.UTF8.GetString(str);
        //}
    }
}
