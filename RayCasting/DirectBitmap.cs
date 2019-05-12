using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Drawing;

namespace OpenSimplexNoiseSample {
    public class DirectBitmap : IDisposable {
        public readonly Bitmap Bitmap;
        public readonly int Width;
        public readonly int Height;
        public readonly byte[] Bits;
        public readonly Size Size;

        private GCHandle bitsHandle;
        private readonly int w4;
        private readonly int bufferSize;

        //private readonly static ImageConverter imgConverter = new ImageConverter();
        //private readonly static Type imgFormat = typeof(byte[]);

        public DirectBitmap(int w, int h) {
            this.Width = w;
            this.Height = h;
            this.Size = new Size(w, h);

            w4 = w * 4;
            bufferSize = w4 * h;
            Bits = new byte[bufferSize];

            bitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            this.Bitmap = new Bitmap(w, h, w4, PixelFormat.Format32bppPArgb, bitsHandle.AddrOfPinnedObject());
        }

        public DirectBitmap(Bitmap bmp) : this(bmp.Width, bmp.Height) {
            BitmapData sourceData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr sourcePointer = sourceData.Scan0;
            int sourceStride = sourceData.Stride;
            int srcBytesPerPixel = sourceStride / bmp.Width;
            int srcOffset;

            int a;
            double pa;

            for(int y = 0; y < bmp.Height; y++) {
                for(int x = 0; x < bmp.Width; x++) {
                    srcOffset = x * srcBytesPerPixel + y * sourceStride;

                    a = (srcBytesPerPixel == 4 ? Marshal.ReadByte(sourcePointer, srcOffset + 3) : 255);
                    pa = a / 255;
                    SetPixel(x, y, Color.FromArgb(a,
                                                 (int)(Marshal.ReadByte(sourcePointer, srcOffset + 2) * pa),
                                                 (int)(Marshal.ReadByte(sourcePointer, srcOffset + 1) * pa),
                                                 (int)(Marshal.ReadByte(sourcePointer, srcOffset + 0) * pa)));
                }
            }

            bmp.UnlockBits(sourceData);
        }

        public Color GetPixel(int x, int y) {
            if(x < 0 || x >= Width || y < 0 || y >= Height) return Color.Black;
            int offset = y * w4 + x * 4;
            return Color.FromArgb(Bits[offset + 3],
                                  Bits[offset + 2],
                                  Bits[offset + 1],
                                  Bits[offset + 0]);
        }

        public void SetPixel(int x, int y, Color value) {
            if(x < 0 || x >= Width || y < 0 || y >= Height) return;
            int offset = y * w4 + x * 4;
            Bits[offset + 3] = value.A;
            Bits[offset + 2] = value.R;
            Bits[offset + 1] = value.G;
            Bits[offset + 0] = value.B;
        }

        public void SetPixel(int x, int y, int value) {
            if(x < 0 || x >= Width || y < 0 || y >= Height) return;
            int offset = y * w4 + x * 4;
            Bits[offset + 3] = 255;
            Bits[offset + 2] = (byte)(value >> 16);
            Bits[offset + 1] = (byte)((value >> 8) & 255);
            Bits[offset + 0] = (byte)(value & 255);
        }

        public void Dispose() {
            Bitmap.Dispose();
            bitsHandle.Free();
        }
    }
}
