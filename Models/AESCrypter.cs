using System;
using System.Collections.Generic;
using System.Linq;
using VoiceTransform.Helpers;

namespace VoiceTransform.Models
{
    public static class AESCrypter
    {
        public static int E_Creation(int p, int q)
        {
            var random = new Random();
            var e = 0;
            var result_check = false;
            var buffer = new List<int>();

            var f = (p - 1) * (q - 1);

            while (!result_check)
            {
                e = random.Next(1, 1000);

                if ((e < f) && PrimeGenerator.IsPrimeNumber(e))
                {
                    var check_simple = IsCoprime(f, e);

                    if (check_simple)
                    {
                        buffer.Add(e);
                    }
                }

                if (buffer.Count == 100)
                {
                    result_check = true;
                }
            }

            var list_e = RemoveDuplicates(buffer);

            return list_e.Min();
        }

        public static List<T> RemoveDuplicates<T>(List<T> list)
        {
            return new HashSet<T>(list).ToList();
        }

        public static int D_Creation(int f, int e)
        {
            var random = new Random();
            var result_check = false;
            var buffer = new List<int>();
            var d = 0;

            while (!result_check)
            {
                d = random.Next(1, 1000);

                if ((d * e) % f == 1)
                {
                    buffer.Add(d);
                }

                if (buffer.Count == 100)
                {
                    result_check = true;
                }
            }

            var list_d = RemoveDuplicates(buffer);

            return list_d.Min();
        }

        private static bool IsCoprime(int num1, int num2)
        {

            if (num1 == num2)
            {
                return num1 == 1;
            }
            else
            {
                if ((num1 > num2 && (num1 - num2 < 0)) || (num1 < num2 && (num2 - num1 < 0)))
                {
                    return false;
                }

                if (num1 > num2)
                {

                    return IsCoprime(num1 - num2, num2);
                }
                else
                {

                    return IsCoprime(num2 - num1, num1);
                }
            }
        }
    }
}

