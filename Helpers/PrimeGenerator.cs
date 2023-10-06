using System;

namespace VoiceTransform.Helpers
{
    public static class PrimeGenerator
    {
        public static int Generate()
        {
            var random = new Random();

            var prime_check = false;

            var value = 1;

            while (!prime_check)
            {
                value = random.Next(1, 100);

                prime_check = IsPrimeNumber(value);
            }

            return value;
        }

        public static bool IsPrimeNumber(int n)
        {
            var result = true;

            if (n > 1)
            {
                for (var i = 2u; i < n; i++)
                {
                    if (n % i == 0)
                    {
                        result = false;
                        break;
                    }
                }
            }
            else
            {
                result = false;
            }

            return result;
        }
    }
}
