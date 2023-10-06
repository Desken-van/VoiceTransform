using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using VoiceTransform.Models;

namespace VoiceTransform.ImageCreation.Converters
{
    public static class BmpToPictureConverter
    {
        public static Bitmap CopyDataToBitmap(byte[] data, int length, string inputImageFileName)
        {
            var resolution = (int)Math.Ceiling(Math.Sqrt(length));

            var delta_res = (int)Math.Pow(resolution, 2) - length;

            Image innocuousBmp = Image.FromFile(inputImageFileName);

            var resize = false;

            if (resolution * 2 > innocuousBmp.Height)
            {
                var kof_value = innocuousBmp.Height;

                innocuousBmp.Dispose();

                Compressor.Resize(inputImageFileName, inputImageFileName, resolution * 4 / kof_value);
                resize = true;
            }
            else if (resolution / 2 > innocuousBmp.Width && !resize)
            {
                var kof_value = innocuousBmp.Width;

                innocuousBmp.Dispose();

                Compressor.Resize(inputImageFileName, inputImageFileName, resolution / kof_value);
            }

            var bmp = new Bitmap(resolution * 2, resolution / 2, PixelFormat.Format32bppArgb);

            var bmpData = bmp.LockBits(
                                 new Rectangle(0, 0, bmp.Width, bmp.Height),
                                 ImageLockMode.WriteOnly, bmp.PixelFormat);

            Marshal.Copy(data, 0, bmpData.Scan0, data.Length);

            bmp.UnlockBits(bmpData);

            return bmp;
        }

        public static byte[] CopyBitmapToData(Bitmap bitmap)
        {
            int width = bitmap.Width,
                height = bitmap.Height;

            var bmpArea = new Rectangle(0, 0, width, height);
            var bmpData = bitmap.LockBits(bmpArea, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var data = new byte[bmpData.Stride * bmpData.Height];

            Marshal.Copy(bmpData.Scan0, data, 0, data.Length);
            bitmap.UnlockBits(bmpData);

            var destination = new List<byte>();
            var leapPoint = width * 2;

            var counter_zero = 0;

            for (int i = 0; i < data.Length; i++)
            {
                if (width % 2 != 0)
                {
                    // Skip at some point
                    if (i == leapPoint)
                    {
                        // Skip 2 bytes since it's 16 bit pixel
                        i += 1;
                        leapPoint += width * 2 + 2;
                        continue;
                    }
                }

                if(i + 2 < data.Length)
                {
                    if (data[i] == 0 && data[i + 1] == 0 && data[i + 2] == 255 && i > 0)
                    {
                        counter_zero++;
                    }
                }

                if (counter_zero == 10)
                {
                    destination.RemoveAt(destination.Count - counter_zero * 4);
                    break;
                }
                else
                {
                    destination.Add(data[i]);
                }
            }
            return destination.ToArray();
        }
    }
}
