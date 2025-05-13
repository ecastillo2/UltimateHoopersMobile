using System;
using System.Security.Cryptography;
using System.Text;

namespace Common.Services
{
    /// <summary>
    /// Service for generating secure random identifiers and passwords
    /// </summary>
    public class SecureIdGeneratorService : ISecureIdGeneratorService
    {
        private static readonly Random _random = new Random();
        private const string AlphanumericChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        /// <summary>
        /// Generate a secure random 8-digit number
        /// </summary>
        public string Generate8Digits()
        {
            var bytes = new byte[4];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            uint random = BitConverter.ToUInt32(bytes, 0) % 100000000;
            return String.Format("{0:D8}", random);
        }

        /// <summary>
        /// Generate a secure random 6-digit number
        /// </summary>
        public string Generate6Digits()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var value = BitConverter.ToInt32(bytes, 0);
            // Ensure the value is positive and within range (100000-999999)
            return Math.Abs(value % 900000 + 100000).ToString();
        }

        /// <summary>
        /// Generate a client ID with CL prefix and 6 digits
        /// </summary>
        public string GenerateClientId()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var value = BitConverter.ToInt32(bytes, 0);
            // Ensure the value is positive and within range (10000-999999)
            return "CL" + Math.Abs(value % 990000 + 10000).ToString();
        }

        /// <summary>
        /// Generate a contractor ID with CTR prefix and 6 digits
        /// </summary>
        public string GenerateContractorId()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var value = BitConverter.ToInt32(bytes, 0);
            // Ensure the value is positive and within range (100000-999999)
            return "CTR" + Math.Abs(value % 900000 + 100000).ToString();
        }

        /// <summary>
        /// Generate a secure random confirmation code with alphanumeric characters
        /// </summary>
        /// <param name="length">Length of the confirmation code</param>
        public string GenerateConfirmationCode(int length = 8)
        {
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = AlphanumericChars[bytes[i] % AlphanumericChars.Length];
            }

            return new string(chars);
        }

        /// <summary>
        /// Generate a secure random password with specified length
        /// </summary>
        /// <param name="length">Length of the password</param>
        public string GenerateRandomPassword(int length = 12)
        {
            if (length < 8)
            {
                throw new ArgumentException("Password length must be at least 8 characters", nameof(length));
            }

            // Define character sets
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specialChars = "!@#$%^&*()_-+=[]{}|;:,.<>?";

            var allChars = uppercase + lowercase + digits + specialChars;
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            var password = new StringBuilder();

            // Ensure at least one of each character type
            password.Append(uppercase[bytes[0] % uppercase.Length]);
            password.Append(lowercase[bytes[1] % lowercase.Length]);
            password.Append(digits[bytes[2] % digits.Length]);
            password.Append(specialChars[bytes[3] % specialChars.Length]);

            // Fill the rest with random characters
            for (int i = 4; i < length; i++)
            {
                password.Append(allChars[bytes[i] % allChars.Length]);
            }

            // Shuffle the password to avoid predictable patterns
            return ShuffleString(password.ToString());
        }

        private string ShuffleString(string input)
        {
            var chars = input.ToCharArray();
            var bytes = new byte[chars.Length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            for (int i = chars.Length - 1; i > 0; i--)
            {
                int j = bytes[i] % (i + 1);
                var temp = chars[i];
                chars[i] = chars[j];
                chars[j] = temp;
            }

            return new string(chars);
        }
    }

    public interface ISecureIdGeneratorService
    {
        string Generate8Digits();
        string Generate6Digits();
        string GenerateClientId();
        string GenerateContractorId();
        string GenerateConfirmationCode(int length = 8);
        string GenerateRandomPassword(int length = 12);
    }
}