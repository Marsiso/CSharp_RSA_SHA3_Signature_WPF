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
        /// <summary>
        /// RSA encryption algorithm.
        /// </summary>
        /// <param name="str">Plain-text to be encrypted.</param>
        /// <param name="e">Part of the public key pair used for encryption.</param>
        /// <param name="n">Part of the public key pair used for encryption.</param>
        /// <returns>Encrypted plain-text.</returns>
        public static string Encrypt(in string str, in BigInteger e, in BigInteger n)
        {
            byte[] encoded = StringUtilities.ToAsciiByteArray(str); // Encode plain-text using ASCII encoding
            string[] encrypted = new string[Convert.ToInt32(Math.Ceiling(str.Length / (double)7))];
            for (int i = 0; i < encrypted.Length; ++i) // Splitting into blocks, 1 block is less or equal 7 characters
            {
                StringBuilder stringBuilder = new StringBuilder(capacity: 77);
                int index = i * 7;
                for (int j = index; j < str.Length && j < index + 7; ++j)
                {
                    _ = stringBuilder.Append(Convert.ToString(encoded[j], 2).PadLeft(11, '0')); // Encode block 8b -> 11b
                }
                encrypted[i] = stringBuilder // Block encryption
                    .ToString()
                    .ToDecimalFromBinary()
                    .ModPow(e, n)
                    .ToString();
            }

            return string.Join(' ', encrypted);
        }

        /// <summary>
        /// Converts binary string to BigInteger.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Binary string as BigInteger.</returns>
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

        /// <summary>
        /// RSA decryption algorithm.
        /// </summary>
        /// <param name="str">Encrypted text (cipher) to be decrypted.</param>
        /// <param name="d">Part of the private key pair used for decryption.</param>
        /// <param name="n">Part of the private key pair used for decryption.</param>
        /// <returns>Decrypted cipher as plain-text.</returns>
        public static string Decrypt(in string str, in BigInteger d, in BigInteger n)
        {
            string[] blocks = str.Split(' '); // Split into blocks
            string[] decoded = new string[blocks.Length];
            for (int i = 0; i < decoded.Length; ++i)
            {
                BigInteger block = new(blocks[i]); // Convert block to integer
                BigInteger decrypted = block.ModPow(d, n); // Block decryption
                decoded[i] = decrypted // Decode block value
                    .ToBinaryFromDecimal()
                    .ToBinary8bFrom11b()
                    .ToASCIIFromBinary();
            }

            return string.Concat(decoded);
        }

        /// <summary>
        /// Converts BigInteger to its binary string representation.
        /// </summary>
        /// <param name="bigInteger"></param>
        /// <returns>BigInteger as binary string.</returns>
        private static string ToBinaryFromDecimal(this BigInteger bigInteger)
        {
            StringBuilder stringBuilder = new();
            foreach (var @byte in bigInteger.ToByteArray())
            {
                _ = stringBuilder.Append(Convert.ToString(@byte, 2).PadLeft(8, '0'));
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Converts ASCII encoded characters with 11b padding back to 8b/1B encoded characters.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>Byte array with each byte representing ASCII encoded character.</returns>
        private static string ToBinary8bFrom11b(this string str)
        {
            StringBuilder stringBuilder = new();
            for (int i = str.Length % 11; i < str.Length; i += 11)
            {
                _ = stringBuilder.Append(str, i + 3, 8);
            }
            
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Decodes ASCII binary string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>Text representation of the currently decrypted block.</returns>
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

        /// <summary>
        /// Generates pair of co-prime numbers.
        /// </summary>
        /// <returns>Tuple(BigInteger, BigInteger) representing co-prime numbers.</returns>
        private static (BigInteger, BigInteger) GeneratePrimeNumbersPQ()
        {
            const int lowerBound = 1024, upperBound = 2048;

            // Randomly chosen prime number length in bits
            int lenBitsP = RandomNumberGenerator.GetInt32(lowerBound, upperBound);
            int lenBitsQ = RandomNumberGenerator.GetInt32(lowerBound, upperBound);

            // Prime number generation
            Random rnd = new();
            BigInteger p = BigInteger.ProbablePrime(lenBitsP, rnd);
            BigInteger q = BigInteger.ProbablePrime(lenBitsQ, rnd);
            while (p.Equals(q))
            {
                q = BigInteger.ProbablePrime(lenBitsQ, rnd);
            }

            return (p, q);
        }

        /// <summary>
        /// Generates random public and private key pairs used for RSA encryption/decryption algorithm.
        /// </summary>
        /// <returns>Tuple(BigInteger, BigInteger, BigInteger) == Tuple(Public Key, Private Key, Shared Key) representing public and private key pair parts.</returns>
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