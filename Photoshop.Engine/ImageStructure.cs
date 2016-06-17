using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Photoshop.Engine
{
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
            _beginImagePointer = bmpData.Scan0;
            _pixels = RemoveStridePadding(image);

            Marshal.Copy(_beginImagePointer, _pixels, 0, _pixels.Length);
            image.UnlockBits(bmpData);
        }

        private byte[] RemoveStridePadding(Bitmap image)
        {
            var bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(image.PixelFormat) / 8;
            var pixels = new byte[image.Width * image.Height * bytesPerPixel];

            for (int i = 0; i < image.Height; i++)
            {
                var memPos = (IntPtr)((long)_beginImagePointer + i * image.Width * bytesPerPixel);
                Marshal.Copy(memPos, pixels, i * image.Width * bytesPerPixel, image.Width * bytesPerPixel);
            }

            return pixels;
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
}
