using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace Photoshop.Engine
{
    public static class ImageManipulator
    {
        public static void ApplyGrayScaleTo(Bitmap image)
        {
            var rectangleToLock = new Rectangle(0, 0, image.Width, image.Height);
            var bmpData = image.LockBits(rectangleToLock, ImageLockMode.ReadWrite, image.PixelFormat);
            var adressFirstLine = bmpData.Scan0;
            var pixelsLength = Math.Abs(bmpData.Stride) * image.Height;
            var rgbValues = new byte[pixelsLength];
            var result = new byte[pixelsLength];

            Marshal.Copy(adressFirstLine, rgbValues, 0, pixelsLength);

            for (int i = 0; i < pixelsLength; i += 3)
            {
                var r = rgbValues[i];
                var g = rgbValues[i + 1];
                var b = rgbValues[i + 2];

                var gray = (byte)((r + g + b) / 3);

                result[i] = gray;
                result[i + 1] = gray;
                result[i + 2] = gray;
            }

            Marshal.Copy(result, 0, adressFirstLine, pixelsLength);
            image.UnlockBits(bmpData);
        }

        public static void ApplyCorrelation(Bitmap image)
        {
            var rectangleToLock = new Rectangle(0, 0, image.Width, image.Height);
            var bmpData = image.LockBits(rectangleToLock, System.Drawing.Imaging.ImageLockMode.ReadWrite, image.PixelFormat);

            var adressFirstLine = bmpData.Scan0;

            var pixelsLength = Math.Abs(bmpData.Stride) * image.Height;
            var rgbValues = new byte[pixelsLength];

            Marshal.Copy(adressFirstLine, rgbValues, 0, pixelsLength);

            var rgbMatrix = TransformArrayByteToColorRepresentationMatrix(rgbValues, image.Width, image.Height);
            //var filter = new float[,] { {1/9 ,1/9 ,1/9},
            //                            {1/9 ,1/9 ,1/9 },
            //                            {1/9 ,1/9 ,1/9 } };

            //ApplyCorrelationOnMatrix(rgbMatrix, filter);
            var result = PixelMatrixToArrayByte(rgbMatrix, pixelsLength);

            for (int i = 0; i < pixelsLength; i++)
            {
                var a = rgbValues[i];
                var b = result[i];

                if(a != b)
                {

                }
            }

            Marshal.Copy(result, 0, adressFirstLine, pixelsLength);
            image.UnlockBits(bmpData);
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
            var rows = matrix.GetLength(1);
            var columns = matrix.GetLength(0);
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

            for (int i = 1; i < rows; i += 4)
            {
                for (int j = 1; j < columns; j += 4)
                {
                    var newR = 0;
                    var newG = 0;
                    var newB = 0;

                    newR += (int)(matrix[i - 1, j - 1].R * filter[0, 0]);
                    newR += (int)(matrix[i - 1, j].R * filter[0, 1]);
                    newR += (int)(matrix[i - 1, j + 1].R * filter[0, 2]);
                    newR += (int)(matrix[i, j - 1].R * filter[1, 0]);
                    newR += (int)(matrix[i, j].R * filter[1, 1]);
                    newR += (int)(matrix[i, j + 1].R * filter[1, 2]);
                    newR += (int)(matrix[i + 1, j - 1].R * filter[2, 0]);
                    newR += (int)(matrix[i + 1, j].R * filter[2, 1]);
                    newR += (int)(matrix[i + 1, j + 1].R * filter[2, 2]);

                    newG += (int)(matrix[i - 1, j - 1].G * filter[0, 0]);
                    newG += (int)(matrix[i - 1, j].G * filter[0, 1]);
                    newG += (int)(matrix[i - 1, j + 1].G * filter[0, 2]);
                    newG += (int)(matrix[i, j - 1].G * filter[1, 0]);
                    newG += (int)(matrix[i, j].G * filter[1, 1]);
                    newG += (int)(matrix[i, j + 1].G * filter[1, 2]);
                    newG += (int)(matrix[i + 1, j - 1].G * filter[2, 0]);
                    newG += (int)(matrix[i + 1, j].G * filter[2, 1]);
                    newG += (int)(matrix[i + 1, j + 1].G * filter[2, 2]);

                    newB += (int)(matrix[i - 1, j - 1].B * filter[0, 0]);
                    newB += (int)(matrix[i - 1, j].B * filter[0, 1]);
                    newB += (int)(matrix[i - 1, j + 1].B * filter[0, 2]);
                    newB += (int)(matrix[i, j - 1].B * filter[1, 0]);
                    newB += (int)(matrix[i, j].B * filter[1, 1]);
                    newB += (int)(matrix[i, j + 1].B * filter[1, 2]);
                    newB += (int)(matrix[i + 1, j - 1].B * filter[2, 0]);
                    newB += (int)(matrix[i + 1, j].B * filter[2, 1]);
                    newB += (int)(matrix[i + 1, j + 1].B * filter[2, 2]);

                    matrix[i, j] = new Pixel(newR, newG, newB);
                }
            }
        }
    }

    public struct Pixel
    {
        private readonly int _r;
        private readonly int _g;
        private readonly int _b;

        public Pixel(int r, int g, int b)
        {
            _r = r;
            _g = g;
            _b = b;
        }

        public int R
        {
            get
            {
                return _r;
            }
        }

        public int G
        {
            get
            {
                return _r;
            }
        }

        public int B
        {
            get
            {
                return _r;
            }
        }

        public override string ToString()
        {
            return string.Format("R:{0} - G:{1} - B:{2}", _r, _g, _b);
        }
    }
}
