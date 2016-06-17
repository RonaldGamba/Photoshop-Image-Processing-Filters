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
        public static void ApplyGrayScaleTo(Bitmap image)
        {
            var imageStructure = new ImageStructure(image);

            for (int i = 0; i < imageStructure.Pixels.Length - 3; i += 3)
            {
                var r = imageStructure.Pixels[i];
                var g = imageStructure.Pixels[i + 1];
                var b = imageStructure.Pixels[i + 2];

                var gray = (byte)((r + g + b) / 3);

                imageStructure.Pixels[i] = gray;
                imageStructure.Pixels[i + 1] = gray;
                imageStructure.Pixels[i + 2] = gray;
            }

            imageStructure.ReprocessImage();
        }

        public static void ApplyCorrelationTo(Bitmap image)
        {
            var imageStructure = new ImageStructure(image);

            var rgbMatrix = TransformArrayByteToColorRepresentationMatrix(imageStructure.Pixels, image.Width, image.Height);

            var filter = new float[,] { { 1/9f ,1/9f ,1/9f},
                                         {1/9f ,1/9f ,1/9f },
                                         {1/9f ,1/9f ,1/9f } };

            ApplyCorrelationOnMatrix(rgbMatrix, filter);
            var result = PixelMatrixToArrayByte(rgbMatrix, imageStructure.Pixels.Length);
            imageStructure.Pixels = result;
            imageStructure.ReprocessImage();
        }

        public static int[] GetImageHistogram(Bitmap image)
        {
            var grayIntensitivity = new int[256];
            var imageStructure = new ImageStructure(image);
            var arrayPixel = TransformArrayByteToColorRepresentationMatrix(imageStructure.Pixels, image.Width, image.Height);

            for (int i = 0; i < imageStructure.Pixels.Length - 3; i += 3)
            {
                var r = imageStructure.Pixels[i];
                var g = imageStructure.Pixels[i + 1];
                var b = imageStructure.Pixels[i + 2];

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
            imageStructure.Pixels = result;
            imageStructure.ReprocessImage();

            return b1;

        }

        private static byte[] GetBytes(Bitmap image)
        {
            var imageStructure = new ImageStructure(image);
            return imageStructure.Pixels;
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
                    matrix[i, j] = new Pixel(r, g, b);
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
                }
            }

            return bytes;
        }

        public static void ApplyCorrelationOnMatrix(Pixel[,] matrix,
                                                    float[,] filter)
        {
            var rows = matrix.GetLength(0);
            var columns = matrix.GetLength(1);

            for (int i = 1; i < rows - 1; i++)
            {
                for (int j = 1; j < columns - 1; j++)
                {
                    double newR = 0;
                    double newG = 0;
                    double newB = 0;

                    newR += (matrix[i - 1, j - 1].R * filter[0, 0]);
                    newR += (matrix[i - 1, j].R * filter[0, 1]);
                    newR += (matrix[i - 1, j + 1].R * filter[0, 2]);
                    newR += (matrix[i, j - 1].R * filter[1, 0]);
                    newR += (matrix[i, j].R * filter[1, 1]);
                    newR += (matrix[i, j + 1].R * filter[1, 2]);
                    newR += (matrix[i + 1, j - 1].R * filter[2, 0]);
                    newR += (matrix[i + 1, j].R * filter[2, 1]);
                    newR += (matrix[i + 1, j + 1].R * filter[2, 2]);

                    newG += (matrix[i - 1, j - 1].G * filter[0, 0]);
                    newG += (matrix[i - 1, j].G * filter[0, 1]);
                    newG += (matrix[i - 1, j + 1].G * filter[0, 2]);
                    newG += (matrix[i, j - 1].G * filter[1, 0]);
                    newG += (matrix[i, j].G * filter[1, 1]);
                    newG += (matrix[i, j + 1].G * filter[1, 2]);
                    newG += (matrix[i + 1, j - 1].G * filter[2, 0]);
                    newG += (matrix[i + 1, j].G * filter[2, 1]);
                    newG += (matrix[i + 1, j + 1].G * filter[2, 2]);

                    newB += (matrix[i - 1, j - 1].B * filter[0, 0]);
                    newB += (matrix[i - 1, j].B * filter[0, 1]);
                    newB += (matrix[i - 1, j + 1].B * filter[0, 2]);
                    newB += (matrix[i, j - 1].B * filter[1, 0]);
                    newB += (matrix[i, j].B * filter[1, 1]);
                    newB += (matrix[i, j + 1].B * filter[1, 2]);
                    newB += (matrix[i + 1, j - 1].B * filter[2, 0]);
                    newB += (matrix[i + 1, j].B * filter[2, 1]);
                    newB += (matrix[i + 1, j + 1].B * filter[2, 2]);

                    matrix[i, j] = new Pixel(newR, newG, newB);
                }
            }
        }
    }
}
