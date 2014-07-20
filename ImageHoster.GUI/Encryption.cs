using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ImageHoster.GUI
{
    public static class Encryption
    {
        static RandomNumberGenerator rng = RNGCryptoServiceProvider.Create();

        public static string GetSalt()
        {
            byte[] salt = new byte[32];
            rng.GetBytes(salt);
            string saltstring = "";

            foreach (byte hex in salt)
                saltstring += hex.ToString("X2", CultureInfo.InvariantCulture.NumberFormat);

            return saltstring;
        }

        public static string ComputeHash(string password, string salt)
        {
            UnicodeEncoding enc = new UnicodeEncoding();
            string concat = password + salt;
            byte[] beforehash = enc.GetBytes(concat);
            var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(beforehash);
            string hashedPassword = "";

            foreach (byte hex in hash)
                hashedPassword += hex.ToString("X2", CultureInfo.InvariantCulture.NumberFormat);

            return hashedPassword;
        }
    }
}