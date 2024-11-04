namespace AonFreelancing.Utilities
{
    public class OTPManager
    {
        private static Random _random = new Random();

        public static string GenerateOtp()
        {
           
            // Generate a random number between 0 and 999999
            int otp = _random.Next(0, (int)Math.Pow(10, 6));

            // Format the number to ensure it's always 6 digits
            return otp.ToString("D6");
        }
    }
}
