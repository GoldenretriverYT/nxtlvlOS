using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Loaders {
    public class NXTBmp {
        public uint[] Data = new uint[0];
        public ushort SizeX, SizeY;

        public NXTBmp(byte[] imgData) {
            SizeX = (ushort)(imgData[0] + (imgData[1] << 8));
            SizeY = (ushort)(imgData[2] + (imgData[3] << 8));

            Data = new uint[SizeX * SizeY];

            Buffer.BlockCopy(imgData, 4, Data, 0, imgData.Length - 4);
        }

        private NXTBmp() { }

        public static NXTBmp FromBitmap(Cosmos.System.Graphics.Bitmap bitmap) {
            var bmp = new NXTBmp();

            bmp.SizeX = (ushort)bitmap.Width;
            bmp.SizeY = (ushort)bitmap.Height;

            bmp.Data = new uint[bmp.SizeX * bmp.SizeY];

            Buffer.BlockCopy(bitmap.RawData, 0, bmp.Data, 0, bmp.Data.Length * 4);

            return bmp;
        }
        
        public NXTBmp Resize(int width, int height) {
            var resized = new NXTBmp();
            resized.SizeX = (ushort)width;
            resized.SizeY = (ushort)height;
            resized.Data = ResizeImage(Data, SizeX, SizeY, width, height);

            return resized;
        }

        private static uint[] ResizeImage(uint[] img, int srcWidth, int srcHeight, int destWidth, int destHeight) {
            uint[] result = new uint[destWidth * destHeight];
            float xRatio = srcWidth / (float)destWidth;
            float yRatio = srcHeight / (float)destHeight;

            for (int y = 0; y < destHeight; y++) {
                for (int x = 0; x < destWidth; x++) {
                    float srcX = x * xRatio;
                    float srcY = y * yRatio;
                    int xFloor = (int)srcX;
                    int yFloor = (int)srcY;
                    int xCeil = xFloor + 1;
                    if (xCeil >= srcWidth) xCeil = xFloor;
                    int yCeil = yFloor + 1;
                    if (yCeil >= srcHeight) yCeil = yFloor;

                    // Get the pixels in four corners
                    uint topLeft = img[yFloor * srcWidth + xFloor];
                    uint topRight = img[yFloor * srcWidth + xCeil];
                    uint bottomLeft = img[yCeil * srcWidth + xFloor];
                    uint bottomRight = img[yCeil * srcWidth + xCeil];

                    // Calculate the weights for each pixel
                    float xFrac = srcX - xFloor;
                    float yFrac = srcY - yFloor;
                    float topWeight = (1 - xFrac);
                    float bottomWeight = xFrac;

                    // Interpolate
                    uint top = Interpolate(topLeft, topRight, topWeight);
                    uint bottom = Interpolate(bottomLeft, bottomRight, bottomWeight);
                    result[y * destWidth + x] = Interpolate(top, bottom, (1 - yFrac));
                }
            }

            return result;
        }

        private static uint Interpolate(uint color1, uint color2, float weight) {
            byte a1 = (byte)((color1 & 0xFF000000) >> 24);
            byte r1 = (byte)((color1 & 0x00FF0000) >> 16);
            byte g1 = (byte)((color1 & 0x0000FF00) >> 8);
            byte b1 = (byte)(color1 & 0x000000FF);

            byte a2 = (byte)((color2 & 0xFF000000) >> 24);
            byte r2 = (byte)((color2 & 0x00FF0000) >> 16);
            byte g2 = (byte)((color2 & 0x0000FF00) >> 8);
            byte b2 = (byte)(color2 & 0x000000FF);

            byte a = (byte)(a1 * weight + a2 * (1 - weight));
            byte r = (byte)(r1 * weight + r2 * (1 - weight));
            byte g = (byte)(g1 * weight + g2 * (1 - weight));
            byte b = (byte)(b1 * weight + b2 * (1 - weight));

            return (uint)((a << 24) | (r << 16) | (g << 8) | b);
        }

    }
}
