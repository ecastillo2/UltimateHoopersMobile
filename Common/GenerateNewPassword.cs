using System.Text;

namespace Common
{
    /// <summary>
    /// Generate Password
    /// </summary>
    public static class GenerateNewPassword
    {
        /// <summary>
        /// Generate Random Password
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder stringBuilder = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(chars[random.Next(chars.Length)]);
            }

            return stringBuilder.ToString();
        }
    }
}
