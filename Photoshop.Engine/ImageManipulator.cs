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

        //private static int[,] waterver(byte[] bytes, int width, int height)
        //{
        //    var rows = bytes.Count() / width;
        //    var columns = bytes.Count() / height;
        //    var currentBytePosition = 0;

        //    var matrix = new int[width, height];
        //    var rowSize = (bytes.Count() / width) * 3;

        //    for (int i = 0; i < width; i++)
        //    {
        //        for (int j = 0; j < height; j++)
        //        {
        //            var 
        //        }
        //    }

        //    return matrix;
        //}

        private static ColorRepresentation[,] TransformArrayByteToColorRepresentationMatrix(byte[] array, int width, int height)
        {
            var matrix = new ColorRepresentation[width,height]();
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var r = 
                }
            }
        }

        public static void ApplyCorrelation(Bitmap image)
        {
            var rectangleToLock = new Rectangle(0, 0, image.Width, image.Height);
            var bmpData = image.LockBits(rectangleToLock, System.Drawing.Imaging.ImageLockMode.ReadWrite, image.PixelFormat);

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

                var gray = (r + g + b) / 3;

                result[i] = (byte)gray;
                result[i + 1] = (byte)gray;
                result[i + 2] = (byte)gray;
            }

            Marshal.Copy(result, 0, adressFirstLine, pixelsLength);
            image.UnlockBits(bmpData);
        }
    }

    struct ColorRepresentation
    {
        int R;
        int G;
        int B;
    }
}
