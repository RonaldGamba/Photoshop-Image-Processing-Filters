using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Photoshop.Engine
{
    public class ImageStructure
    {
        private byte[] _pixels;

        public ImageStructure(Bitmap sourceBitmap)
        {
            var bmpData = sourceBitmap.LockBits(new Rectangle(0, 0,
                                    sourceBitmap.Width, sourceBitmap.Height),
                                    ImageLockMode.ReadOnly,
                                    PixelFormat.Format32bppArgb);

            _pixels = new byte[bmpData.Stride * bmpData.Height];
            Marshal.Copy(bmpData.Scan0, _pixels, 0, _pixels.Length);
            sourceBitmap.UnlockBits(bmpData);
        }

        public byte[] BufferPixels
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
