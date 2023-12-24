namespace JTProtocol;

public static class JProtocolEncryptor
{
    private static string Key => "2e985f930";

    public static byte[] Encrypt(byte[] data) => RijndaelHandler.Encrypt(data, Key);

    public static byte[] Decrypt(byte[] data) => RijndaelHandler.Decrypt(data, Key);
}