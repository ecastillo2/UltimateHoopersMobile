// File: WebAPI/Services/PasswordService.cs
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace WebAPI.Services
{
    /// <summary>
    /// Service for handling password operations
    /// </summary>
    public class PasswordService : IPasswordService
    {
        private const int MinLength = 8;
        private const int MaxLength = 128;

        /// <summary>
        /// Validates a password against security requirements
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>True if password meets requirements</returns>
        public bool ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            if (password.Length < MinLength || password.Length > MaxLength)
                return false;

            // Check for at least one digit
            if (!Regex.IsMatch(password, @"\d"))
                return false;

            // Check for at least one uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;

            // Check for at least one lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
                return false;

            // Check for at least one special character
            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]"))
                return false;

            return true;
        }

        /// <summary>
        /// Generates a cryptographically secure random password
        /// </summary>
        /// <param name="length">Password length</param>
        /// <returns>Secure random password</returns>
        public string GenerateSecurePassword(int length = 16)
        {
            if (length < MinLength)
                length = MinLength;

            const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
            const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numberChars = "0123456789";
            const string specialChars = "!@#$%^&*()_-+=[{]};:>|./?";

            var passwordChars = new char[length];
            var rng = RandomNumberGenerator.Create();
            var buffer = new byte[length];

            // Ensure at least one of each character type
            passwordChars[0] = GetRandomChar(lowerChars, rng);
            passwordChars[1] = GetRandomChar(upperChars, rng);
            passwordChars[2] = GetRandomChar(numberChars, rng);
            passwordChars[3] = GetRandomChar(specialChars, rng);

            // Fill the rest with random chars from all char sets
            var allChars = lowerChars + upperChars + numberChars + specialChars;
            for (int i = 4; i < length; i++)
            {
                passwordChars[i] = GetRandomChar(allChars, rng);
            }

            // Shuffle the password characters
            ShuffleArray(passwordChars, rng);

            return new string(passwordChars);
        }

        /// <summary>
        /// Get a random character from the specified character set
        /// </summary>
        private char GetRandomChar(string charSet, RandomNumberGenerator rng)
        {
            var buffer = new byte[1];
            rng.GetBytes(buffer);
            var index = buffer[0] % charSet.Length;
            return charSet[index];
        }

        /// <summary>
        /// Shuffle array using Fisher-Yates algorithm
        /// </summary>
        private void ShuffleArray<T>(T[] array, RandomNumberGenerator rng)
        {
            var buffer = new byte[1];
            for (int i = array.Length - 1; i > 0; i--)
            {
                rng.GetBytes(buffer);
                var j = buffer[0] % (i + 1);
                var temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }
    }

    /// <summary>
    /// Interface for password service
    /// </summary>
    public interface IPasswordService
    {
        bool ValidatePassword(string password);
        string GenerateSecurePassword(int length = 16);
    }
}