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

            int i;

            if(image.RawFormat.Guid == ImageFormat.Jpeg.Guid)
            {
                i = 0;
            }
            else
            {
                i = 8;
            }

            for (i = 0; i < imageStructure.Pixels.Length; i += 3)
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

        public static Bitmap BitwiseOperation(Bitmap b1, Bitmap b2, Func<int, int, byte> operation)
        {
            if (b1.Width != b2.Width || b1.Height != b2.Height)
                throw new ArgumentException("The size of the images must be the same.");

            var b1Bytes = GetBytes(b1);
            var b2Bytes = GetBytes(b2);
            var result = new byte[b1Bytes.Length];

            for (int i = 0; i < b1Bytes.Length; i++)
            {
                var v1 = (int)b1Bytes[i];
                var v2 = (int)b2Bytes[i];
                result[i] = operation(v1,v2);
            }

            var imageStructure = new ImageStructure(b1);
            imageStructure.Pixels = result;
            imageStructure.ReprocessImage();

            return b1;
        }

        private static byte[] GetBytes(Bitmap image)
        {
            var rectangleToLock = new Rectangle(0, 0, image.Width, image.Height);
            var bmpData = image.LockBits(rectangleToLock, System.Drawing.Imaging.ImageLockMode.ReadWrite, image.PixelFormat);

            var adressFirstLine = bmpData.Scan0;

            var pixelsLength = Math.Abs(bmpData.Stride) * image.Height;
            var rgbValues = new byte[pixelsLength];

            Marshal.Copy(adressFirstLine, rgbValues, 0, pixelsLength);
            image.UnlockBits(bmpData);
            return rgbValues;
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

    public class ImageStructure
    {
        private IntPtr _beginImagePointer;
        private byte[] _pixels;
        private Bitmap _image;

        public ImageStructure(Bitmap image)
        {
            _image = image;

            var rectangleToLock = new Rectangle(0, 0, image.Width, image.Height);
            var bmpData = image.LockBits(rectangleToLock, ImageLockMode.ReadWrite, image.PixelFormat);
            var pixelsLength = Math.Abs(bmpData.Stride) * image.Height;

            _beginImagePointer = bmpData.Scan0;
            _pixels = new byte[pixelsLength];

            Marshal.Copy(_beginImagePointer, _pixels, 0, pixelsLength);
            image.UnlockBits(bmpData);
        }
                
        public void ReprocessImage()
        {
            Marshal.Copy(_pixels, 0, _beginImagePointer, _pixels.Length);
        }

        public byte[] Pixels
        {
            get
            {
                return _pixels;
            }
            set
            {
                _pixels = value;
            }
        }
    }

    public struct Pixel
    {
        private readonly double _r;
        private readonly double _g;
        private readonly double _b;

        public Pixel(double r, double g, double b)
        {
            _r = r;
            _g = g;
            _b = b;
        }

        public double R
        {
            get
            {
                return _r;
            }
        }

        public double G
        {
            get
            {
                return _g;
            }
        }

        public double B
        {
            get
            {
                return _b;
            }
        }

        public override string ToString()
        {
            return string.Format("R:{0} - G:{1} - B:{2}", _r, _g, _b);
        }
    }
}
