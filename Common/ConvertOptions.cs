using System.Security.Cryptography;

namespace Common
{
    public static class ConvertOptions
    {
        /// <summary>
        /// YesNoOptions
        /// </summary>
        /// <returns></returns>
        public static string YesNoOptions()
        {
            var bytes = new byte[4];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            uint random = BitConverter.ToUInt32(bytes, 0) % 100000000;
            return String.Format("{0:D8}", random);
        }

        /// <summary>
        /// Generate Six Digit
        /// </summary>
        /// <returns></returns>
        public static string GenerateSixDigit()
        {
            Random rnd = new Random();
            string num = rnd.Next(100000, 999999).ToString();

            return num;
        }

        /// <summary>
        /// Client Generate Six Digit
        /// </summary>
        /// <returns></returns>
		public static string ClientGenerateSixDigit()
		{
			Random rnd = new Random();
			string num = rnd.Next(10000, 999999).ToString();

			return "CL" + num;
		}

        /// <summary>
        /// Contractor Generate Six Digit
        /// </summary>
        /// <returns></returns>
		public static string ContractorGenerateSixDigit()
		{
			Random rnd = new Random();
			string num = rnd.Next(100000, 999999).ToString();

			return "CTR"+num;
		}

	}
}
