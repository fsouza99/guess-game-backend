using System.Security.Cryptography;

namespace App.StaticTools;

public static class DataGen
{
    public static string StringID()
    {
        const string alphanum = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        const int size = 11;

        var buffer = new char[size];
        for (int i = 0; i < size; i++)
        {
            buffer[i] = alphanum[RandomNumberGenerator.GetInt32(62)];
        }

        return new String(buffer);
    }

    public static string AppUserNick()
    {
        const string alphanum = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const int suffixSize = 8;

        var buffer = new char[suffixSize];
        for (int i = 0; i < suffixSize; i++)
        {
            buffer[i] = alphanum[RandomNumberGenerator.GetInt32(36)];
        }
        var suffix = new String(buffer);
        var nick = $"User{suffix}";

        return nick;
    }
}
