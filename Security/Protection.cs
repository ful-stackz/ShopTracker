using System;
using System.Text;
using System.Security.Cryptography;

namespace ShopTracker.Security
{
    public static class Protection
    {
        public static string MakeSalt(int length = 32)
        {
            using (var secureRander = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[length];
                secureRander.GetNonZeroBytes(salt);

                return Convert.ToBase64String(salt);
            }
        }

        public static string HashPassword(string password, string salt)
        {
            byte[] _salt = Encoding.UTF8.GetBytes(salt);
            byte[] _pass = Encoding.UTF8.GetBytes(password);

            using (var hasher = new HMACSHA512(_salt))
            {
                byte[] secret = hasher.ComputeHash(_pass);

                return Convert.ToBase64String(secret);
            }
        }
    }
}
