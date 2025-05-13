
namespace Common
{
    public class GenerateConfirmationCode
    {
        /// <summary>
        /// Get Confimation Code
        /// </summary>
        /// <returns></returns>
        public static string GetConfimationCode()
        {
            var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var Charsarr = new char[8];
            var random = new Random();

            for (int i = 0; i < Charsarr.Length; i++)
            {
                Charsarr[i] = characters[random.Next(characters.Length)];
            }

            var resultString = new String(Charsarr);

            return resultString.ToString();
        }

    }
}
