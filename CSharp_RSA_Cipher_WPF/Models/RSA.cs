using Org.BouncyCastle.Math;
using System;
using System.Security.Cryptography;
using System.Text;
using BigIntegerMath = Org.BouncyCastle.Utilities.BigIntegers;
using StringUtilities = Org.BouncyCastle.Utilities.Strings;

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

            return string.Join(' ', encrypted);
        }

        private static BigInteger ToDecimalFromBinary(this string value)
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
                BigInteger block = new(blocks[i]);
                BigInteger decrypted = block.ModPow(d, n);
                decoded[i] = decrypted
                    .ToBinaryFromDecimal()
                    .ToBinary8bFrom11b()
                    .ToASCIIFromBinary();
            }

            return string.Concat(decoded);
        }

        private static string ToBinaryFromDecimal(this BigInteger bigInteger)
        {
            StringBuilder stringBuilder = new();
            foreach (var @byte in bigInteger.ToByteArray())
            {
                _ = stringBuilder.Append(Convert.ToString(@byte, 2).PadLeft(8, '0'));
            }

            return stringBuilder.ToString();
        }

        private static string ToBinary8bFrom11b(this string str)
        {
            StringBuilder stringBuilder = new();
            for (int i = str.Length % 11; i < str.Length; i += 11)
            {
                _ = stringBuilder.Append(str, i + 3, 8);
            }
            
            return stringBuilder.ToString();
        }

        private static string ToASCIIFromBinary(this string str)
        {
            var len = str.Length / 8;
            byte[] bytes = new byte[len];
            for (int i = 0; i < len; i++)
            {
                bytes[i] = Convert.ToByte(str.Substring(i * 8, 8), 2);
            }
            return StringUtilities.FromAsciiByteArray(bytes);
        }

        private static (BigInteger, BigInteger) GeneratePrimeNumbersPQ()
        {
            const int lowerBound = 2048, upperBound = 3072;

            int lenBitsP = RandomNumberGenerator.GetInt32(lowerBound, upperBound);
            int lenBitsQ = RandomNumberGenerator.GetInt32(lowerBound, upperBound);

            Random rnd = new();
            BigInteger p = BigInteger.ProbablePrime(lenBitsP, rnd);
            BigInteger q = BigInteger.ProbablePrime(lenBitsQ, rnd);
            while (p.Equals(q))
            {
                q = BigInteger.ProbablePrime(lenBitsQ, rnd);
            }

            return (p, q);
        }

        public static (BigInteger, BigInteger, BigInteger) GenerateKeyPairs()
        {
            BigInteger publicKey, privateKey, sharedKey;
            // Prime numbers and phi
            var (p, q) = GeneratePrimeNumbersPQ();
            var phi = p.Add(BigIntegerMath.One.Negate()).Multiply(q.Add(BigIntegerMath.One.Negate()));

            // Shared key
            sharedKey = p.Multiply(q);

            // Public key
            Org.BouncyCastle.Security.SecureRandom random = new();
            BigInteger upperBound = phi.Add(BigIntegerMath.One.Negate());
            do
            {
                publicKey = BigIntegerMath.CreateRandomInRange(BigInteger.Two, upperBound, random);
            }
            while (!publicKey.Gcd(phi).Equals(BigInteger.One));

            // Private key
            privateKey = publicKey.ModInverse(phi);

            return (publicKey, privateKey, sharedKey);
        }
    }
}