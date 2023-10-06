using System.Drawing.Drawing2D;
using System.Drawing;

namespace VoiceTransform.ImageCreation
{
    public static class Compressor
    {
        public static void Resize(string imageFile, string outputFile, double scaleFactor)
        {
            using (var srcImage = Image.FromFile(imageFile))
            {
                var newWidth = (int)(srcImage.Width * scaleFactor);
                var newHeight = (int)(srcImage.Height * scaleFactor);

                var bitmap = new Bitmap(newWidth, newHeight);
                var graphics = Graphics.FromImage(bitmap);

                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                graphics.DrawImage(srcImage, new Rectangle(0, 0, newWidth, newHeight));

                graphics.Dispose();
                srcImage.Dispose();

                bitmap.Save(outputFile);

                //using (var newImage = new Bitmap(newWidth, newHeight))
                //using (var graphics = Graphics.FromImage(newImage))
                ///{
                //graphics.SmoothingMode = SmoothingMode.AntiAlias;
                //graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //graphics.DrawImage(srcImage, new Rectangle(0, 0, newWidth, newHeight));
                ////newImage.Save(outputFile);
                ///}
            }
        }
    }
}
