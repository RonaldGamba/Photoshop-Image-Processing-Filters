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
            var imageStructure = new ImageStructure(image);

            for (int i = 0; i < imageStructure.BufferPixels.Length - 4; i += 4)
            {
                var r = imageStructure.BufferPixels[i];
                var g = imageStructure.BufferPixels[i + 1];
                var b = imageStructure.BufferPixels[i + 2];

                var gray = (byte)((r + g + b) / 3);

                imageStructure.BufferPixels[i] = gray;
                imageStructure.BufferPixels[i + 1] = gray;
                imageStructure.BufferPixels[i + 2] = gray;
            }

            return BitmapHelper.CreateNewBitmapFrom(image, imageStructure.BufferPixels);
        }

        public static int[] GenerateImageHistogram(Bitmap image)
        {
            var grayIntensitivity = new int[256];
            var imageStructure = new ImageStructure(image);
            var arrayPixel = TransformArrayByteToColorRepresentationMatrix(imageStructure.BufferPixels, image.Width, image.Height);

            for (int i = 0; i < imageStructure.BufferPixels.Length - 3; i += 3)
            {
                var r = imageStructure.BufferPixels[i];
                var g = imageStructure.BufferPixels[i + 1];
                var b = imageStructure.BufferPixels[i + 2];

                var grayScale = (byte)((r + g + b) / 3);
                grayIntensitivity[grayScale]++;
            }

            return grayIntensitivity;
        }

        public static Bitmap BitwiseOperation(Bitmap b1, Bitmap b2, Func<int, int, byte> operation)
        {

            var b1Bytes = GetBytes(b1);
            var b2Bytes = GetBytes(b2);
            var result = new byte[Math.Min(b1Bytes.Count(), b2Bytes.Count())];

            for (int i = 0; i < result.Length; i++)
            {
                var v1 = (int)b1Bytes[i];
                var v2 = (int)b2Bytes[i];
                result[i] = operation(v1, v2);
            }

            var imageStructure = new ImageStructure(b1);
            imageStructure.BufferPixels = result;

            return b1;

        }

        private static byte[] GetBytes(Bitmap image)
        {
            var imageStructure = new ImageStructure(image);
            return imageStructure.BufferPixels;
        }

        public static Pixel[,] TransformArrayByteToColorRepresentationMatrix(byte[] array, int width, int height)
        {
            var matrix = new Pixel[height, width];
            var positionOnArray = 0;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var r = array[positionOnArray++];
                    var g = array[positionOnArray++];
                    var b = array[positionOnArray++];
                    var a = array[positionOnArray++];
                    matrix[i, j] = new Pixel(r, g, b, a);
                }
            }

            return matrix;
        }

        public static byte[] PixelMatrixToArrayByte(Pixel[,] matrix, int pixelLength)
        {
            var rows = matrix.GetLength(0);
            var columns = matrix.GetLength(1);
            var bytes = new byte[pixelLength];
            var arrayBytePosition = 0;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    bytes[arrayBytePosition++] = (byte)matrix[i, j].R;
                    bytes[arrayBytePosition++] = (byte)matrix[i, j].G;
                    bytes[arrayBytePosition++] = (byte)matrix[i, j].B;
                    bytes[arrayBytePosition++] = (byte)matrix[i, j].A;
                }
            }

            return bytes;
        }

        public static Bitmap ConvolutionFilter(Bitmap sourceBitmap, float[,] filter)
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
                    var centralPixelPos = (sourceData.Stride * imageOffsetX) + (imageOffsetY * 4);

                    var blue = 0d;
                    var green = 0d;
                    var red = 0d;

                    for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                    {
                        for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                        {
                            var currentPixelBasPos = (sourceData.Stride * (imageOffsetX + filterX)) + (filterY + imageOffsetY) * 4;

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

        public static Bitmap ConvolutionFilter(this Bitmap sourceBitmap,
                                           float[,] xFilterMatrix,
                                           float[,] yFilterMatrix,
                                                 double factor = 1,
                                                      int bias = 0,
                                            bool grayscale = false)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,
                                     sourceBitmap.Width, sourceBitmap.Height),
                                                       ImageLockMode.ReadOnly,
                                                  PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);

            if (grayscale == true)
            {
                float rgb = 0;

                for (int k = 0; k < pixelBuffer.Length; k += 4)
                {
                    rgb = pixelBuffer[k] * 0.11f;
                    rgb += pixelBuffer[k + 1] * 0.59f;
                    rgb += pixelBuffer[k + 2] * 0.3f;

                    pixelBuffer[k] = (byte)rgb;
                    pixelBuffer[k + 1] = pixelBuffer[k];
                    pixelBuffer[k + 2] = pixelBuffer[k];
                    pixelBuffer[k + 3] = 255;
                }
            }

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

            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0,
                                     resultBitmap.Width, resultBitmap.Height),
                                                      ImageLockMode.WriteOnly,
                                                  PixelFormat.Format32bppArgb);

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);

            return resultBitmap;
        }

        public static Bitmap ApplyHighPassFilter(Bitmap image)
        {
            //var filter = new float[,]
            //    { {-1/9f,-1/9f,-1/9f,},
            //      {-1/9f,  8,  -1/9f,},
            //      {-1/9f,-1/9f,-1/9f,}, };

            var filter = new float[,]
                { {0,-1/4f,0,},
                  {-1/4f,  2,  -1/4f,},
                  {0,-1/4f,0,}, };

            return ConvolutionFilter(image, filter);
        }

        public static Bitmap ApplyLowPassFilter(Bitmap sourceBitmap)
        {
            var filter = new float[,] {  {1/9f ,1/9f ,1/9f},
                                         {1/9f ,1/9f ,1/9f },
                                         {1/9f ,1/9f ,1/9f } };

            return ConvolutionFilter(sourceBitmap, filter);
        }

        public static Bitmap ApplyPrewitt(Bitmap sourceBitmap)
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

        public static Bitmap ApplySobel(Bitmap sourceBitmap)
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

        public static Bitmap ApplyRobert(Bitmap sourceBitmap)
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
    }
}
