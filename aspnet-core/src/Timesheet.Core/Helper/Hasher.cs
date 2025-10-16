using System;
using System.Security.Cryptography;
using System.Text;

namespace Timesheet.Helper
{
    public static class Hasher
    {
        public static byte[] HMAC_SHA256(byte[] key, byte[] data)
        {
            using (var hmac = new HMACSHA256(key))
            {
                return hmac.ComputeHash(data);
            }
        }

        public static string HEX(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", "").ToLower();
        }
        public static string EncodeBase64(this string value)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(valueBytes);
        }

        public static string DecodeBase64(this string value)
        {
            var valueBytes = System.Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(valueBytes);
        }

        public static byte[] MD5Hash(byte[] data)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                return md5.ComputeHash(data);
            }
        }

        public static string MD5Hash(string input)
        {
            var data = Encoding.UTF8.GetBytes(input);
            var hashBytes = MD5Hash(data);
            return HEX(hashBytes);
        }
    }
}
