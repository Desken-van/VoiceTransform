using NAudio.Wave;
using System;
using System.Drawing;
using System.Linq;
using VoiceTransform.Helpers;
using VoiceTransform.ImageCreation;
using VoiceTransform.ImageCreation.Converters;

namespace VoiceTransform.Models
{
    public static class VoiceCrypter
    {
        public static AES_DataModel Crypt_Voice(string voicePath, int srate, int p, int q)
        {
            var reader = new WaveFileReader(voicePath);

            var bytesBuffer = new byte[reader.Length]; 
            reader.Read(bytesBuffer, 0, bytesBuffer.Length);

            var crypted_bytes = bytesBuffer;

            var result = ColorCalculator.Audio_to_Image(crypted_bytes, srate, p, q);

            return result;     
        }

        public static RGB_DataModel Crypt_Voice(string voicePath, int srate)
        {
            var reader = new WaveFileReader(voicePath);

            var bytesBuffer = new byte[reader.Length];
            reader.Read(bytesBuffer, 0, bytesBuffer.Length);

            var crypted_bytes = bytesBuffer;

            var result = ColorCalculator.Audio_to_Image(crypted_bytes, srate);

            return result;
        }

        public static byte[] Uncrypt_Voice(string filename, int n, int lenght)
        {
            var bmp_uncrypt = new Bitmap(filename);

            var taken_bytes = BmpToPictureConverter.CopyBitmapToData(bmp_uncrypt);

            bmp_uncrypt.Dispose();

            var end = taken_bytes.ToList().LastIndexOf(253);

            var d_byte = new byte[] 
            { 
                taken_bytes[end - 4], 
                taken_bytes[end - 3], 
                taken_bytes[end - 2], 
                taken_bytes[end - 1],
            };

            var d = BitConverter.ToInt32(d_byte, 0);

            Array.Resize(ref taken_bytes, taken_bytes.Length - (taken_bytes.Length - end) - 4);

            var check = BitConverter.GetBytes(164705);

            var p_count_byte = new byte[]
            {
                taken_bytes[taken_bytes.Length - 4],
                taken_bytes[taken_bytes.Length - 3],
                taken_bytes[taken_bytes.Length - 2],
                taken_bytes[taken_bytes.Length - 1],
            };

            var packet_count = BitConverter.ToInt32(p_count_byte, 0); 

            Array.Resize(ref taken_bytes, taken_bytes.Length - 4);

            var bytes_buffer = ColorCalculator.Image_to_Audio(taken_bytes, d, n, packet_count);

            Array.Resize(ref bytes_buffer, bytes_buffer.Length - (bytes_buffer.Length - lenght - 9));

            var uncrypted_bytes = bytes_buffer;

            return uncrypted_bytes;
        }

        public static byte[] Uncrypt_Voice(string filename, int lenght)
        {
            var bmp_uncrypt = new Bitmap(filename);

            var taken_bytes = BmpToPictureConverter.CopyBitmapToData(bmp_uncrypt);

            bmp_uncrypt.Dispose();

            var bytes_buffer = ColorCalculator.Image_to_Audio(taken_bytes);

            Array.Resize(ref bytes_buffer, bytes_buffer.Length - 2 * (bytes_buffer.Length - lenght));

            var uncrypted_bytes = bytes_buffer;

            return uncrypted_bytes;
        }
    }
}
