using System.Security.Cryptography;

namespace JTProtocol;

public static class RijndaelHandler
{
    private const int Keysize = 128;
    private const int DerivationIterations = 1000;

    public static byte[] Encrypt(byte[] data, string passPhrase)
    {
        var saltStringBytes = Generate128BitsOfRandomEntropy();
        var ivStringBytes = Generate128BitsOfRandomEntropy();

        using var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations);
        var keyBytes = password.GetBytes(Keysize / 8);
        using var symmetricKey = new RijndaelManaged();
        symmetricKey.BlockSize = 128;
        symmetricKey.Mode = CipherMode.CBC;
        symmetricKey.Padding = PaddingMode.PKCS7;
        using var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes);
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        cryptoStream.Write(data, 0, data.Length);
        cryptoStream.FlushFinalBlock();
        var cipherTextBytes = saltStringBytes;
        cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
        cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
        memoryStream.Close();
        cryptoStream.Close();
        return cipherTextBytes;
    }

    public static byte[] Decrypt(byte[] data, string passPhrase)
    {
        var saltStringBytes = data.Take(Keysize / 8).ToArray();
        var ivStringBytes = data.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
        var cipherTextBytes = data.Skip(Keysize / 8 * 2).Take(data.Length - Keysize / 8 * 2).ToArray();

        using var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations);
        var keyBytes = password.GetBytes(Keysize / 8);
        using var symmetricKey = new RijndaelManaged();
        symmetricKey.BlockSize = 128;
        symmetricKey.Mode = CipherMode.CBC;
        symmetricKey.Padding = PaddingMode.PKCS7;
        using var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes);
        using var memoryStream = new MemoryStream(cipherTextBytes);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        var plainTextBytes = new byte[cipherTextBytes.Length];
        var read = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
        memoryStream.Close();
        cryptoStream.Close();
        return plainTextBytes.Take(read).ToArray();
    }


    private static byte[] Generate128BitsOfRandomEntropy()
    {
        var randomBytes = new byte[16];
        using var rngCsp = new RNGCryptoServiceProvider();
        rngCsp.GetBytes(randomBytes);
        return randomBytes;
    }
}