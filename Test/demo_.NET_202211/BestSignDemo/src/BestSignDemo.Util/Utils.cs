using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using XC.RSAUtil;

namespace BestSignDemo.Util
{
    class ParamChecker
    {
        public static bool CheckIsNotEmpty(ICollection param)
        {
            return param != null && param.Count != 0;
        }

        public static bool CheckIsNotBlank(string param)
        {
            return param != null && param.Trim().Length != 0;
        }
    }

    class CryptUtils
    {
        public static string MD5Hash(string source)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(source));

                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    stringBuilder.Append(data[i].ToString("x2"));
                }
                return stringBuilder.ToString();
            }
        }

        public static string RSA256Sign(string source, string privateKey)
        {
            RsaPkcs8Util rsa = new RsaPkcs8Util(Encoding.UTF8, null, privateKey);
            byte[] data = rsa.SignDataGetBytes(source, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            string signature = Convert.ToBase64String(data);
            signature = UrlEncode(signature);
            return signature;
        }

        public static string UrlEncode(string source)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < source.Length; i++)
            {
                string c = source[i].ToString();
                string k = HttpUtility.UrlEncode(c);
                if (c == k)
                {
                    stringBuilder.Append(c);
                }
                else
                {
                    stringBuilder.Append(k.ToUpper());
                }
            }
            return stringBuilder.ToString();
        }
    }

    class ConfigLoader
    {
        private static Dictionary<string, string> configs;

        static ConfigLoader()
        {
            configs = new Dictionary<string, string>();

            string[] configLines = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "../../../src/Demo.ini");
            foreach(string line in configLines)
            {
                string trimedLine = line.Trim();
                int spliterIndex = trimedLine.IndexOf('=');
                if (spliterIndex > 0)
                {
                    string key = trimedLine.Substring(0, spliterIndex).Trim();
                    string value = trimedLine.Substring(spliterIndex + 1, line.Length - spliterIndex -1).Trim();
                    configs.Add(key, value);
                }
            }
        }

        public static string GetConfig(string key)
        {
            string value = configs.GetValueOrDefault(key);
            if (!ParamChecker.CheckIsNotBlank(value))
            {
                throw new Exception($"The config value of '{key}' is required.");
            }
            return value;
        }
    }
}
