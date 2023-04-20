using System;
using System.Security.Cryptography;
using System.Text;

namespace SugarTalk.Core.Extensions;

public static class CryptographyExtension
{
    public static string ToSha256(this string input)
    {
        using var sha256 = SHA256.Create();
        return Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(input)));
    } 
}