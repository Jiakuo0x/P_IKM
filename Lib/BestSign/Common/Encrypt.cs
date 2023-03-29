using System.Security.Cryptography;
using System.Text;

namespace Lib.BestSign.Common;

public static class Encrypt
{
    public static string SignStr(string signStr, string privateKeyStr)
    {
        SHA256 sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(signStr));

        var privateKey = Convert.FromBase64String(privateKeyStr);
        var rsa = new RSACryptoServiceProvider();
        rsa.ImportPkcs8PrivateKey(privateKey, out _);

        var encryptedBytes = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var encryptedBase64 = Convert.ToBase64String(encryptedBytes);
        var encryptedUrlCode = System.Web.HttpUtility.UrlEncode(encryptedBase64, Encoding.UTF8);
        return encryptedUrlCode;
    }
}