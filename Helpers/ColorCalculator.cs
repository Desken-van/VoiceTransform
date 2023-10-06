using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Windows.Forms;
using VoiceTransform.Models;

namespace VoiceTransform.Helpers
{
    public static class ColorCalculator
    {
        public static AES_DataModel Audio_to_Image(byte[] bytesBuffer, int srate , int p, int q)
        {
            var n = p * q; 
            var f = (p - 1) * (q - 1);

            var e = AESCrypter.E_Creation(p, q);
            var d = AESCrypter.D_Creation(f, e);

            var new_arr = Get_Str_mas(bytesBuffer, srate);

            var rgb_data = RGB_data(new_arr, e, n); 

            var result = new AES_DataModel()
            {
                ByteArray = rgb_data.ByteArray,   
                Value = d,
                Count = rgb_data.Count,     
            };

            return result;
        }

        public static RGB_DataModel Audio_to_Image(byte[] bytesBuffer, int srate)
        {
            var new_arr = Get_Str_mas(bytesBuffer, srate);

            var rgb_data = RGB_data(new_arr);

            var result = rgb_data;

            return result;
        }

        private static List<string> Get_Str_mas(byte[] bytesBuffer, int srate)
        {
            var new_arr = new List<string>();

            var buffer_symbols = new string[] { "a", "b", "c", "d", "e" };

            var random = new Random();

            for (int i = 0; i < bytesBuffer.Length; i++)
            {
                var gate = random.Next(0, 1) == 1;

                var salt_pos = "";
                var salt_neg = "";

                for (int j = 0; j < 6 - bytesBuffer[i].ToString().Length; j++)
                {
                    salt_pos += buffer_symbols[random.Next(0, 4)];
                    salt_neg += buffer_symbols[random.Next(0, 4)];
                }

                var value = bytesBuffer[i];
                var int_value = Convert.ToInt32(value.ToString());

                //var result_value = (int)(BigInteger.Pow(int_value, e) % n); 

                var result_value = value;

                if (result_value >= 0)
                {
                    if (gate)
                    {
                        var app = $"#{result_value}" + salt_pos; //#125fcde
                        new_arr.Add(app);
                    }
                    else
                    {
                        var app = $"#{salt_pos}{result_value}";
                        new_arr.Add(app);
                    }
                }
                else
                {
                    if (gate)
                    {
                        var app = $"#f{result_value * -1}" + salt_neg;
                        new_arr.Add(app);
                    }
                    else
                    {
                        var app = $"#f{salt_neg}{result_value * -1}";
                        new_arr.Add(app);
                    }
                }
            }

            return new_arr;
        }

        private static RGB_DataModel RGB_data(List<string> data, int e, int n)
        {
            var byte_mas = new List<byte>();
            var count_numbers = new List<int>();//5350 323 875
            var counter = 0;

            foreach (var hex in data)
            {
                var color = ColorTranslator.FromHtml(hex); 

                counter = ColorCrypting(color.A, byte_mas, e, n, counter, count_numbers);
                counter = ColorCrypting(color.R, byte_mas, e, n, counter, count_numbers);
                counter = ColorCrypting(color.G, byte_mas, e, n, counter, count_numbers);
                counter = ColorCrypting(color.B, byte_mas, e, n, counter, count_numbers);
            }

            var list = ""; 

            byte_mas.Add(254);

            foreach (var num in count_numbers)
            {
                list = num.ToString();

                foreach (char ch in list.ToCharArray())
                {
                    var value = Convert.ToByte(ch - '0');

                    byte_mas.Add(value);
                }

                //byte_mas.Add(Convert.ToByte(200 + num.ToString().Length));

                byte_mas.Add(253);
            }

            var number = BitConverter.GetBytes(count_numbers.Count); //164705

            foreach(var part in number)
            {
                byte_mas.Add(part);
            }

            var result = new RGB_DataModel()
            {
                ByteArray = byte_mas.ToArray(),
                Count = count_numbers.Count,
            };

            return result;
        }

        private static RGB_DataModel RGB_data(List<string> data)
        {
            var byte_mas = new List<byte>();
            var count_numbers = new List<int>();//5350 323 875
            var counter = 0;

            foreach (var hex in data)
            {
                var color = ColorTranslator.FromHtml(hex);

                byte_mas.Add(Convert.ToByte(color.A));
                byte_mas.Add(Convert.ToByte(color.R));
                byte_mas.Add(Convert.ToByte(color.G));
                byte_mas.Add(Convert.ToByte(color.B));
            }

            var result = new RGB_DataModel()
            {
                ByteArray = byte_mas.ToArray(),
                Count = count_numbers.Count,
            };

            return result;
        }

        private static int ColorCrypting(byte color, List<byte> byte_mas, int e, int n, int counter, List<int> count_numbers)
        {
            var crypted = (int)(BigInteger.Pow(color, e) % n);

            if(crypted < 256)
            {
                byte_mas.Add(Convert.ToByte(crypted));
            }
            else
            {
                byte_mas.Add(color);

                count_numbers.Add(counter);
            }

            return counter + 1;
        }

        public static byte[] Image_to_Audio(byte[] taken_bytes, int d, int n, int packet_count)
        {
            var hex_list = Data_From_RGB(taken_bytes, d, n, packet_count);

            var data = Get_Byte_Buffer(hex_list);

            return data;
        }

        public static byte[] Image_to_Audio(byte[] taken_bytes)
        {
            var hex_list = Data_From_RGB(taken_bytes);

            var data = Get_Byte_Buffer(hex_list);

            return data;
        }

        private static List<string> Data_From_RGB(byte[] data, int d, int n, int packet_count)
        {
            var tick = data.ToList().LastIndexOf(254);
            var list = "";

            var num_list = new List<int>();  

            Console.WriteLine("Progress:");
            Console.WriteLine("");

            var progresser_value = packet_count / 100;
            var progresser = packet_count / 100;
            var progresser_per = 0;

            for (int i = tick + 1; i < data.Length - tick; i++)
            {
                if(num_list.Count == packet_count)
                {
                    break;
                }
                if (data[i] == 253)
                {
                    num_list.Add(Convert.ToInt32(list));    

                    list= "";
                }
                else
                {
                    list += data[i].ToString();
                }

                if ((i - tick) - (progresser_value * progresser_per) >= progresser)
                {
                    //progresser = progresser_value + (i - tick);
                    progresser_per++;

                    Console.WriteLine($"{progresser_per}%");
                }
            }

            var datalist = data.ToList();
            datalist.RemoveRange(tick, data.Length - tick);

            var clear_list = datalist.ToArray();

            var str_list = new List<string>();

            for (int i = 0; i < clear_list.Length; i += 4)
            {
                int decrypted_a = ColorDecrypting(clear_list[i], i, num_list, d, n);
                int decrypted_r = ColorDecrypting(clear_list[i+1], i+1, num_list, d, n);
                int decrypted_g = ColorDecrypting(clear_list[i+2], i+2, num_list, d, n);
                int decrypted_b = ColorDecrypting(clear_list[i+3], i+3, num_list, d, n);

                //var a = Convert.ToInt32(data[i]);
                //var r = Convert.ToInt32(data[i + 1]);
                //var g = Convert.ToInt32(data[i + 2]);
                //var b = Convert.ToInt32(data[i + 3]);

                //var decrypted_a = (int)(BigInteger.Pow(a, d) % n);
                //var decrypted_r = (int)(BigInteger.Pow(r, d) % n);
                //var decrypted_g = (int)(BigInteger.Pow(g, d) % n);
                //var decrypted_b = (int)(BigInteger.Pow(b, d) % n);

                var color = Color.FromArgb(decrypted_a, decrypted_r, decrypted_g, decrypted_b);

                //var color = Color.FromArgb(a, r, g, b);

                var RGB = ColorTranslator.ToHtml(color);

                str_list.Add(RGB);
            }

            return str_list;
        }

        private static List<string> Data_From_RGB(byte[] data)
        {
            var num_list = new List<int>();

            var str_list = new List<string>();

            for (int i = 0; i < data.Length; i += 4)
            {
                var a = Convert.ToInt32(data[i]);
                var r = Convert.ToInt32(data[i + 1]);
                var g = Convert.ToInt32(data[i + 2]);
                var b = Convert.ToInt32(data[i + 3]);

                var color = Color.FromArgb(a, r, g, b);

                var RGB = ColorTranslator.ToHtml(color);

                str_list.Add(RGB);
            }

            return str_list;
        }

        private static int ColorDecrypting(byte data, int index, List<int> num_list, int d, int n)
        {
            int result;

            var value = Convert.ToInt32(data);

            if (num_list.Contains(index))
            {
                result = value;
            }
            else
            {
                result = (int)(BigInteger.Pow(value, d) % n);
            }
            
            if(result > 255)
            {
                return value;
            }
            
            return result;
        }

        private static byte[] Get_Byte_Buffer(List<string> hex_list)
        {
            var byte_buffer = new List<byte>();

            var num_dic = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            foreach (var hex in hex_list)
            {
                int value;

                int.TryParse(string.Join("", hex.Where(c => char.IsDigit(c))), out value);

                var check = true;

                if (value == 0)
                {
                    check = false;

                    foreach (var c in num_dic)
                    {
                        if (hex.Contains(c))
                        {
                            check = true;
                        }
                    }
                }

                //var decrypted_value = (int)(BigInteger.Pow(value, d) % n);

                if(value < 256)
                {
                    var byte_val = Convert.ToByte(value);

                    byte_buffer.Add(byte_val);
                }
            }

            return byte_buffer.ToArray();
        }
    }
}
