using System;
using System.Collections;
using System.Drawing.Imaging;
using System.Drawing;
using VoiceTransform.Models;
using VoiceTransform.ImageCreation.Converters;

namespace VoiceTransform.ImageCreation
{
    public class ImageHider
    {
        public static void Encode(byte[] hiddenBytes, string inputImageFileName, string outputImageFileName)
        {
            byte[] hiddenLengthBytes = BitConverter.GetBytes(hiddenBytes.Length); // отримання довжини повідомлення

            byte[] hiddenCombinedBytes = BmpToByteArreyConverter.Combine(hiddenLengthBytes, hiddenBytes); //додавання довжини масиву на початок байтового масиву

            Image innocuousBmp = Image.FromFile(inputImageFileName); // отримання BMP зображення

            byte[] rgbComponents = BmpToByteArreyConverter.RgbComponentsToBytes(innocuousBmp); // отримання байтового маисиву контейнеру

            byte[] encodedRgbComponents = EncodeBytes(hiddenCombinedBytes, rgbComponents); // додавання бітів повідомлення до байтів контейнера

            Bitmap encodedBmp = BmpToByteArreyConverter.ByteArrayToBitmap(encodedRgbComponents, innocuousBmp.Width, innocuousBmp.Height);

            encodedBmp.Save(outputImageFileName, ImageFormat.Png);
        }

        private static byte[] EncodeBytes(byte[] hiddenBytes, byte[] innocuousBytes)
        {
            BitArray hiddenBits = new BitArray(hiddenBytes);
            byte[] encodedBitmapRgbComponents = new byte[innocuousBytes.Length];

            for (int i = 0; i < innocuousBytes.Length; i++)
            {

                if (i < hiddenBits.Length)
                {
                    byte evenByte = (byte)(innocuousBytes[i] - innocuousBytes[i] % 2);
                    encodedBitmapRgbComponents[i] = (byte)(evenByte + (hiddenBits[i] ? 1 : 0));
                }

                else
                {
                    encodedBitmapRgbComponents[i] = innocuousBytes[i];
                }
            }

            return encodedBitmapRgbComponents;
        }

        public static Packet_Info Decode(string imageFileName)
        {
            Bitmap loadedEncodedBmp = new Bitmap(imageFileName);
            byte[] loadedEncodedRgbComponents = BmpToByteArreyConverter.RgbComponentsToBytes(loadedEncodedBmp);

            const int bytesInInt = 4;
            byte[] loadedHiddenLengthBytes = DecodeBytes(loadedEncodedRgbComponents, 0, bytesInInt);
            int loadedHiddenLength = BitConverter.ToInt32(loadedHiddenLengthBytes, 0);

            byte[] loadedHiddenBytes = DecodeBytes(loadedEncodedRgbComponents, bytesInInt, loadedHiddenLength);

            return new Packet_Info
            {
                Data = loadedHiddenBytes,
                Lenght = loadedHiddenLength,
            };
        }

        private static byte[] DecodeBytes(byte[] innocuousLookingData, int byteIndex, int byteCount)
        {
            const int bitsInBytes = 8;
            var bitCount = byteCount * bitsInBytes;
            var bitIndex = byteIndex * bitsInBytes;
            bool[] loadedHiddenBools = new bool[bitCount];

            for (int i = 0; i < bitCount; i++)
            {
                if (i + bitIndex < innocuousLookingData.Length)
                {
                    loadedHiddenBools[i] = innocuousLookingData[i + bitIndex] % 2 == 1;
                }

                //loadedHiddenBools[i] = innocuousLookingData[i + bitIndex] % 2 == 1;
            }

            BitArray loadedHiddenBits = new BitArray(loadedHiddenBools);
            byte[] loadedHiddenBytes = new byte[loadedHiddenBits.Length / bitsInBytes];

            loadedHiddenBits.CopyTo(loadedHiddenBytes, 0);
            return loadedHiddenBytes;
        }

        public static void CreateMask(string inputImageFileName1, string inputImageFileName2)
        {
            Image image1 = Image.FromFile(inputImageFileName1);
            Image image2 = Image.FromFile(inputImageFileName2);
            Bitmap bmp1 = new Bitmap(image1);
            Bitmap bmp2 = new Bitmap(image2);
            Bitmap maskDiff = new Bitmap(bmp1);
            Bitmap maskParity1 = new Bitmap(bmp1);
            Bitmap maskParity2 = new Bitmap(bmp2);

            for (int i = 0; i < maskDiff.Height; i++)
            {
                for (int j = 0; j < maskDiff.Width; j++)
                {
                    Color px1 = bmp1.GetPixel(j, i);
                    Color px2 = bmp2.GetPixel(j, i);

                    int maskDiffIntensity = 255 - Math.Abs(px2.R - px1.R) * 85 - Math.Abs(px2.G - px1.G) * 85 - Math.Abs(px2.B - px1.B) * 85;
                    maskDiff.SetPixel(j, i, Color.FromArgb(maskDiffIntensity, maskDiffIntensity, maskDiffIntensity));

                    int maskParityIntensity1 = px1.R % 2 * 85 + px1.G % 2 * 85 + px1.B % 2 * 85;
                    maskParity1.SetPixel(j, i, Color.FromArgb(maskParityIntensity1, maskParityIntensity1, maskParityIntensity1));

                    int maskParityIntensity2 = px2.R % 2 * 85 + px2.G % 2 * 85 + px2.B % 2 * 85;
                    maskParity2.SetPixel(j, i, Color.FromArgb(maskParityIntensity2, maskParityIntensity2, maskParityIntensity2));
                }
            }

            maskDiff.Save("maskDiff.png");
            maskParity1.Save("F:\\Diplom\\VoiceTransform\\Buffer\\Uncrypted\\maskParity_test.png");
            maskParity2.Save("F:\\Diplom\\VoiceTransform\\Buffer\\Uncrypted\\maskParity_result.png");
        }
    }
}
