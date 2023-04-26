using System.Security.Cryptography;
using System.Text;

namespace Lib.BestSign.Common;

/// <summary>
/// Encrypt for Bestsign
/// </summary>
public static class Encrypt
{
    /// <summary>
    /// Sign the string that needs to be signed using a private key string.
    /// </summary>
    /// <param name="signStr">String that needs to be signed</param>
    /// <param name="privateKeyStr">Private key string of Bestsign</param>
    /// <returns>Sign</returns>
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