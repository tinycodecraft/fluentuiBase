﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace fluentuiBase.Shared.Tools
{
    public sealed class PasswordHasher
    {
        public byte Version => 1;
        public int SaltSize { get; } = 128 / 8; // 128 bits
        public HashAlgorithmName HashAlgorithmName { get; } = HashAlgorithmName.SHA256;

        public string HashPassword(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            // The salt must be unique for each password
            byte[] salt = GenerateSalt(SaltSize);
            byte[] hash = HashPasswordWithSalt(password, salt);

            var inArray = new byte[1 + SaltSize + hash.Length];
            inArray[0] = Version;
            Buffer.BlockCopy(salt, 0, inArray, 1, SaltSize);
            Buffer.BlockCopy(hash, 0, inArray, 1 + SaltSize, hash.Length);

            return Convert.ToBase64String(inArray);
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            if (hashedPassword == null)
                return PasswordVerificationResult.Failed;

            byte[] numArray = Convert.FromBase64String(hashedPassword);
            if (numArray.Length < 1)
                return PasswordVerificationResult.Failed;

            byte version = numArray[0];
            if (version > Version)
                return PasswordVerificationResult.Failed;

            var salt = numArray.Skip(1).Take(SaltSize).ToArray();
            var bytes = numArray.Skip(1 + SaltSize).ToArray();

            var hash = HashPasswordWithSalt(password, salt);

            if (FixedTimeEquals(hash, bytes))
                return PasswordVerificationResult.Success;

            return PasswordVerificationResult.Failed;
        }

        private byte[] HashPasswordWithSalt(string password, byte[] salt)
        {
            byte[] hash;
            using (var hashAlgorithm = SHA256.Create()) //HashAlgorithm.Create(HashAlgorithmName.Name))
            {
                byte[] input = Encoding.UTF8.GetBytes(password);
                hashAlgorithm.TransformBlock(salt, 0, salt.Length, salt, 0);
                hashAlgorithm.TransformFinalBlock(input, 0, input.Length);
                hash = hashAlgorithm.Hash;
            }

            return hash;
        }

        private static byte[] GenerateSalt(int byteLength)
        {
            //using (var cryptoServiceProvider = new RNGCryptoServiceProvider())
            //{
            //    var data = new byte[byteLength];
            //    cryptoServiceProvider.GetBytes(data);
            //    return data;
            //}

            var data = new byte[byteLength];
            RandomNumberGenerator.Fill(data);
            //cryptoServiceProvider.GetBytes(data);
            return data;
        }

        // In .NET Core 2.1, you can use CryptographicOperations.FixedTimeEquals
        // https://github.com/dotnet/runtime/blob/419e949d258ecee4c40a460fb09c66d974229623/src/libraries/System.Security.Cryptography.Primitives/src/System/Security/Cryptography/CryptographicOperations.cs#L32
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            // NoOptimization because we want this method to be exactly as non-short-circuiting as written.
            // NoInlining because the NoOptimization would get lost if the method got inlined.
            if (left.Length != right.Length)
            {
                return false;
            }

            int length = left.Length;
            int accum = 0;

            for (int i = 0; i < length; i++)
            {
                accum |= left[i] - right[i];
            }

            return accum == 0;
        }
    }

    public enum PasswordVerificationResult
    {
        Failed,
        Success,
        SuccessRehashNeeded,
    }
}
