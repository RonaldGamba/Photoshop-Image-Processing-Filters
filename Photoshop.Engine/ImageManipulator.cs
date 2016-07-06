using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;

namespace Photoshop.Engine
{
    public static class ImageManipulator
    {
        private static int BYTES_PER_PIXEL = 4;

        public static Bitmap ApplyGrayScaleTo(Bitmap image)
        {
            var pixelBuffer = ReadImageBytes(image);
            var resultBuffer = new byte[pixelBuffer.Length];

            for (int i = 0; i < pixelBuffer.Length - BYTES_PER_PIXEL; i += BYTES_PER_PIXEL)
            {
                var r = pixelBuffer[i];
                var g = pixelBuffer[i + 1];
                var b = pixelBuffer[i + 2];

                var gray = (byte)((r + g + b) / 3);

                resultBuffer[i] = gray;
                resultBuffer[i + 1] = gray;
                resultBuffer[i + 2] = gray;
                resultBuffer[i + 3] = 255;
            }

            return BitmapHelper.CreateNewBitmapFrom(image, resultBuffer);
        }

        public static float[] GenerateImageHistogram(Bitmap image)
        {
            var grayIntensitivity = new float[256];
            var pixelBuffer = ReadImageBytes(image);

            for (int i = 0; i < pixelBuffer.Length - 3; i += 3)
            {
                var r = pixelBuffer[i];
                var g = pixelBuffer[i + 1];
                var b = pixelBuffer[i + 2];

                var grayScale = (byte)((r + g + b) / 3);
                grayIntensitivity[grayScale]++;
            }

            return grayIntensitivity;
        }

        public static float[] EqualizeHistogram(Bitmap image)
        {
            var histogram = GenerateImageHistogram(image);
            var probabilityPixels = new float[256];
            var cumulativeProbability = new float[256];
            var result = new float[256];

            for (int i = 0; i < histogram.Length; i++)
                probabilityPixels[i] = histogram[i] / (image.Width * image.Height);

            for (int i = 0; i < probabilityPixels.Length; i++)
            {
                if (i == 0)
                    cumulativeProbability[0] = probabilityPixels[0];
                else
                    for (int j = i; j >= 0; j--)
                        cumulativeProbability[i] += probabilityPixels[j];

                cumulativeProbability[i] *= 7;
                result[i] = (float)Math.Floor(cumulativeProbability[i]);
            }

            return result;
        }

        private static byte[] ReadImageBytes(Bitmap image)
        {
            var sourceData = image.LockBits(
               new Rectangle(0, 0,
               image.Width,
               image.Height),
               ImageLockMode.ReadOnly,
               PixelFormat.Format32bppArgb);

            var pixelBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            image.UnlockBits(sourceData);

            return pixelBuffer;
        }

        public static Bitmap ApplyLaplacianFilter(Bitmap image)
        {
            var filter = new float[,]
                { {0,-1f,0,},
                  {-1f,  4,  -1f,},
                  {0,-1f,0,}, };

            return ConvolutionFilter(image, filter);
        }

        public static Bitmap ApplyHighPassFilter(Bitmap image)
        {
            var filter = new float[,]
                { {-1f,-1f,-1f,},
                  {-1f,  8,-1f,},
                  {-1f,-1f,-1f,}, };

            return ConvolutionFilter(image, filter);
        }

        public static Bitmap ApplyLowPassFilter(Bitmap sourceBitmap)
        {
            var filter = new float[,] {  {1/9f ,1/9f ,1/9f},
                                         {1/9f ,1/9f ,1/9f },
                                         {1/9f ,1/9f ,1/9f } };

            return ConvolutionFilter(sourceBitmap, filter);
        }

        public static Bitmap ApplyMedianPassFilter(Bitmap sourceBitmap, eMedianFilterQuality quality)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,
                              sourceBitmap.Width, sourceBitmap.Height),
                                                ImageLockMode.ReadOnly,
                                          PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);

            var qualityValue = (int)(quality);
            var neighborsOffset = qualityValue / 2;
            var mediumValueIndex = (qualityValue * qualityValue + 1) / 2;

            for (int imageOffsetX = neighborsOffset; imageOffsetX < sourceBitmap.Height - neighborsOffset; imageOffsetX++)
            {
                for (int imageOffsetY = neighborsOffset; imageOffsetY < sourceBitmap.Width - neighborsOffset; imageOffsetY++)
                {
                    var centralPixelPos = (sourceData.Stride * imageOffsetX) + (imageOffsetY * BYTES_PER_PIXEL);

                    var blueValues = new byte[qualityValue * qualityValue];
                    var greenValues = new byte[qualityValue * qualityValue];
                    var redValues = new byte[qualityValue * qualityValue];
                    var posOnArray = 0;

                    for (int offsetX = -neighborsOffset; offsetX <= neighborsOffset; offsetX++)
                    {
                        for (int offsetY = -neighborsOffset; offsetY <= neighborsOffset; offsetY++)
                        {
                            var currentPixelPos = (sourceData.Stride * (imageOffsetX + offsetX)) + (offsetY + imageOffsetY) * BYTES_PER_PIXEL;

                            blueValues[posOnArray] = pixelBuffer[currentPixelPos];
                            greenValues[posOnArray] = pixelBuffer[currentPixelPos + 1];
                            redValues[posOnArray] = pixelBuffer[currentPixelPos + 2];

                            posOnArray++;
                        }
                    }

                    resultBuffer[centralPixelPos] = blueValues.OrderBy(v => v).ElementAt(mediumValueIndex);
                    resultBuffer[centralPixelPos + 1] = greenValues.OrderBy(v => v).ElementAt(mediumValueIndex);
                    resultBuffer[centralPixelPos + 2] = redValues.OrderBy(v => v).ElementAt(mediumValueIndex);
                    resultBuffer[centralPixelPos + 3] = 255;
                }
            }

            return BitmapHelper.CreateNewBitmapFrom(sourceBitmap, resultBuffer);
        }

        public static Bitmap ApplyPrewittFilter(Bitmap sourceBitmap)
        {
            var filterV = new float[,]
                { {  1,  1,  1, },
                  {  0,  0,  0, },
                  { -1, -1, -1, }, };

            var filterH = new float[,]
                { { -1,  0,  1, },
                  { -1,  0,  1, },
                  { -1,  0,  1, }, };

            return ConvolutionFilter(sourceBitmap, filterV, filterH);

        }

        public static Bitmap ApplySobelFilter(Bitmap sourceBitmap)
        {
            var filterV = new float[,]
               { {  -1,  0, 1, },
                  { -2,  0, 2, },
                  { -1, -0, 1, }, };

            var filterH = new float[,]
                { { -1,  -2, -1, },
                  {  0,   0,  0, },
                  {  1,  2, 1, }, };

            return ConvolutionFilter(sourceBitmap, filterV, filterH);
        }

        public static Bitmap ApplyRobertFilter(Bitmap sourceBitmap)
        {
            var filterV = new float[,]
               { {  0,  0, -1, },
                  { 0,  1, 0, },
                  { 0, -0, 0, }, };

            var filterH = new float[,]
                { { -1,  0, 0, },
                  {  0,  1, 0, },
                  {  0,  0, 0, }, };

            return ConvolutionFilter(sourceBitmap, filterV, filterH);
        }

        private static Bitmap ConvolutionFilter(Bitmap sourceBitmap, 
                                                float[,] filter)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,
                                     sourceBitmap.Width, sourceBitmap.Height),
                                                       ImageLockMode.ReadOnly,
                                                 PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);

            int filterHeight = filter.GetLength(0);
            int filterWidth = filter.GetLength(1);
            int filterOffset = (filterWidth - 1) / 2;

            for (int imageOffsetX = 1; imageOffsetX < sourceBitmap.Height - 1; imageOffsetX++)
            {
                for (int imageOffsetY = 1; imageOffsetY < sourceBitmap.Width - 1; imageOffsetY++)
                {
                    var centralPixelPos = (sourceData.Stride * imageOffsetX) + (imageOffsetY * BYTES_PER_PIXEL);

                    var blue = 0d;
                    var green = 0d;
                    var red = 0d;

                    for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                    {
                        for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                        {
                            var currentPixelBasPos = (sourceData.Stride * (imageOffsetX + filterX)) + (filterY + imageOffsetY) * BYTES_PER_PIXEL;

                            blue += pixelBuffer[currentPixelBasPos] * filter[filterX + filterOffset, filterY + filterOffset];
                            green += pixelBuffer[currentPixelBasPos + 1] * filter[filterX + filterOffset, filterY + filterOffset];
                            red += pixelBuffer[currentPixelBasPos + 2] * filter[filterX + filterOffset, filterY + filterOffset];
                        }
                    }

                    if (blue > 255)
                    { blue = 255; }
                    else if (blue < 0)
                    { blue = 0; }

                    if (green > 255)
                    { green = 255; }
                    else if (green < 0)
                    { green = 0; }

                    if (red > 255)
                    { red = 255; }
                    else if (red < 0)
                    { red = 0; }

                    resultBuffer[centralPixelPos] = (byte)blue;
                    resultBuffer[centralPixelPos + 1] = (byte)green;
                    resultBuffer[centralPixelPos + 2] = (byte)red;
                    resultBuffer[centralPixelPos + 3] = 255;
                }
            }

            return BitmapHelper.CreateNewBitmapFrom(sourceBitmap, resultBuffer);
        }

        private static Bitmap ConvolutionFilter(this Bitmap sourceBitmap,
                                           float[,] xFilterMatrix,
                                           float[,] yFilterMatrix)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,
                                     sourceBitmap.Width, sourceBitmap.Height),
                                                       ImageLockMode.ReadOnly,
                                                  PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);

            double blueX = 0.0;
            double greenX = 0.0;
            double redX = 0.0;

            double blueY = 0.0;
            double greenY = 0.0;
            double redY = 0.0;

            double blueTotal = 0.0;
            double greenTotal = 0.0;
            double redTotal = 0.0;

            int filterOffset = 1;
            int calcOffset = 0;

            int byteOffset = 0;

            for (int offsetY = filterOffset; offsetY <
                sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX <
                    sourceBitmap.Width - filterOffset; offsetX++)
                {
                    blueX = greenX = redX = 0;
                    blueY = greenY = redY = 0;

                    blueTotal = greenTotal = redTotal = 0.0;

                    byteOffset = offsetY *
                                 sourceData.Stride +
                                 offsetX * 4;

                    for (int filterY = -filterOffset;
                        filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset;
                            filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset +
                                         (filterX * 4) +
                                         (filterY * sourceData.Stride);

                            blueX += (double)(pixelBuffer[calcOffset]) *
                                      xFilterMatrix[filterY + filterOffset,
                                              filterX + filterOffset];

                            greenX += (double)(pixelBuffer[calcOffset + 1]) *
                                      xFilterMatrix[filterY + filterOffset,
                                              filterX + filterOffset];

                            redX += (double)(pixelBuffer[calcOffset + 2]) *
                                      xFilterMatrix[filterY + filterOffset,
                                              filterX + filterOffset];

                            blueY += (double)(pixelBuffer[calcOffset]) *
                                      yFilterMatrix[filterY + filterOffset,
                                              filterX + filterOffset];

                            greenY += (double)(pixelBuffer[calcOffset + 1]) *
                                      yFilterMatrix[filterY + filterOffset,
                                              filterX + filterOffset];

                            redY += (double)(pixelBuffer[calcOffset + 2]) *
                                      yFilterMatrix[filterY + filterOffset,
                                              filterX + filterOffset];
                        }
                    }

                    blueTotal = Math.Sqrt((blueX * blueX) + (blueY * blueY));
                    greenTotal = Math.Sqrt((greenX * greenX) + (greenY * greenY));
                    redTotal = Math.Sqrt((redX * redX) + (redY * redY));

                    if (blueTotal > 255)
                    { blueTotal = 255; }
                    else if (blueTotal < 0)
                    { blueTotal = 0; }

                    if (greenTotal > 255)
                    { greenTotal = 255; }
                    else if (greenTotal < 0)
                    { greenTotal = 0; }

                    if (redTotal > 255)
                    { redTotal = 255; }
                    else if (redTotal < 0)
                    { redTotal = 0; }

                    resultBuffer[byteOffset] = (byte)(blueTotal);
                    resultBuffer[byteOffset + 1] = (byte)(greenTotal);
                    resultBuffer[byteOffset + 2] = (byte)(redTotal);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }

            return BitmapHelper.CreateNewBitmapFrom(sourceBitmap, resultBuffer);
        }
    }

    public enum eMedianFilterQuality
    {
        Low3x3 = 3,
        Medium5x5 = 5,
        High7x7 = 7
    }
}
