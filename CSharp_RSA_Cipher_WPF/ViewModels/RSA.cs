using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Math;
using System.Text;
using StringUtilities = Org.BouncyCastle.Utilities.Strings;
using BigIntegerMath = Org.BouncyCastle.Utilities.BigIntegers;

namespace CSharp_RSA_Cipher_WPF.ViewModels
{
    public static class RSA
    {
        public static string Encrypt(in string str, in BigInteger e, in BigInteger n)
        {
            byte[] encoded = StringUtilities.ToAsciiByteArray(str);
            string[] encrypted = new string[Convert.ToInt32(Math.Ceiling(str.Length / (double)7))];
            for (int i = 0; i < encrypted.Length; ++i)
            {
                StringBuilder stringBuilder = new StringBuilder(capacity: 77);
                int index = i * 7;
                for (int j = index; j < str.Length && j < index + 7; ++j)
                {
                    _ = stringBuilder.Append(Convert.ToString(encoded[j], 2).PadLeft(11, '0'));
                }
                encrypted[i] = stringBuilder
                    .ToString()
                    .ToDecimalFromBinary()
                    .ModPow(e, n)
                    .ToString();
            }

            return String.Join(' ', encrypted);
        }

        public static BigInteger ToDecimalFromBinary(this string value)
        {
            BigInteger bigInteger = BigInteger.Zero;
            foreach (char c in value)
            {
                bigInteger = bigInteger.ShiftLeft(1);
                bigInteger = bigInteger.Add(c is '1' ? BigInteger.One : BigInteger.Zero);
            }

            return bigInteger;
        }

        public static string Decrypt(in string str, in BigInteger d, in BigInteger n)
        {
            string[] blocks = str.Split(' ');
            string[] decoded = new string[blocks.Length];
            for (int i = 0; i < decoded.Length; ++i)
            {
                BigInteger block = new BigInteger(blocks[i]);
                BigInteger decrypted = block.ModPow(d, n);
                decoded[i] = decrypted
                    .ToBinaryFromDecimal()
                    .ToBinary8bFrom11b()
                    .ToASCIIFromBinary();
            }

            return String.Join(string.Empty, decoded);
        }

        public static string ToBinaryFromDecimal(this BigInteger bigInteger)
        {
            var stringBuilder = new StringBuilder();
            byte[] bytes = bigInteger.ToByteArray();
            foreach (var b in bytes)
            {
                _ = stringBuilder.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }

            return stringBuilder.ToString().PadLeft(80, '0');
        }

        public static string ToBinary8bFrom11b(this string str)
        {
            StringBuilder stringBuilder = new StringBuilder(capacity: 56);
            for (int i = 3; i < str.Length; i += 11)
            {
                for (int j = i + 3; j < i + 11; ++j)
                {
                    _ = stringBuilder.Append(str[j]);
                }
            }

            return stringBuilder.ToString();
        }

        public static string ToASCIIFromBinary(this string str)
        {
            byte[] bytes = new byte[7];
            for (int i = 0; i < 7; ++i)
            {
                StringBuilder stringBuilder = new StringBuilder(capacity: 8);
                for (int j = i * 8; j < (i * 8) + 8; ++j)
                {
                    _ = stringBuilder.Append(str[j]);
                }
                string @byte = stringBuilder.ToString();
                bytes[i] = Convert.ToByte(@byte, 2);
            }
            string decoded = StringUtilities.FromAsciiByteArray(bytes);
            return decoded;
        }

        public static BigInteger GenereatePrivateKey(in BigInteger e, in BigInteger Φ) => e.ModInverse(Φ);

        public static BigInteger GenereatePublicKey(in BigInteger Φ)
        {
            BigInteger e;
            Org.BouncyCastle.Security.SecureRandom random = new();
            BigInteger upperBound = Φ.Add(BigIntegerMath.One.Negate());
            do
                e = BigIntegerMath.CreateRandomInRange(BigInteger.Two, upperBound, random);
            while (!e.Gcd(Φ).Equals(BigInteger.One));

            return e;
        }

        public static BigInteger GetN(in BigInteger p, in BigInteger q) => p.Multiply(q);

        public static BigInteger GetΦ(in BigInteger p, in BigInteger q) => p
            .Add(BigIntegerMath.One.Negate())
            .Multiply(q.Add(BigIntegerMath.One.Negate()));

        public static (BigInteger, BigInteger) GetPQ()
        {
            const int lowerBound = 2048, upperBound = 4096;

            int lenBitsP = RandomNumberGenerator.GetInt32(lowerBound, upperBound);
            int lenBitsQ = RandomNumberGenerator.GetInt32(lowerBound, upperBound);

            Random rnd = new();
            BigInteger p = BigInteger.ProbablePrime(lenBitsP, rnd);
            BigInteger q = BigInteger.ProbablePrime(lenBitsQ, rnd);
            while (p.Equals(q))
                q = BigInteger.ProbablePrime(lenBitsQ, rnd);
            return (p, q);
        }
    }
}
