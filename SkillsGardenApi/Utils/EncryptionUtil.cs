using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SkillsGardenApi.Utils
{
    public static class EncryptionUtil
    {
        public static byte[] Hash(string value, byte[] salt)
        {
            return Hash(Encoding.UTF8.GetBytes(value), salt);
        }

        private static byte[] Hash(byte[] value, byte[] salt)
        {
            byte[] saltedValue = value.Concat(salt).ToArray();
            return new SHA256Managed().ComputeHash(saltedValue);
        }

        public static byte[] CreateSalt()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[16];
            rng.GetBytes(buff);
            return buff;
        }


        public static bool Verify(string password, byte[] passwordDb, byte[] saltDb)
        {
            byte[] passwordHash = Hash(password, saltDb);
            return passwordDb.SequenceEqual(passwordHash);
        }
    }
}
