using nxtlvlOS.Windowing.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing {
    public static class WindowManager {
        public static IRenderTarget Target;
        public static uint[] Buffer;
        public static uint[] EmptyBuffer;

        private static List<Form> forms = new();
        private static uint sizeX, sizeY;

        public static void Init() {
            (sizeX, sizeY) = Target.GetSize();
            Buffer = new uint[sizeX * sizeY];
            EmptyBuffer = new uint[sizeX * sizeY];
        }

        public static void Update() {
            // Clear buffer
            System.Buffer.BlockCopy(EmptyBuffer, 0, Buffer, 0, Buffer.Length*4);

            foreach(var form in forms) {
                form.Update();
                form.BufferWasUpdated = false;

                #region Copy Buffer
                if (form.DrawMode == BufferDrawMode.RawCopy) {
                    uint offsetInThisElement = (uint)((form.RelativePosY * sizeX) + form.RelativePosX);
                    uint offsetInChild = 0;

                    for (var y = 0; y < form.SizeY; y++) {
                        System.Buffer.BlockCopy(form.Buffer, (int)offsetInChild*4, Buffer, (int)offsetInThisElement * 4, (int)form.SizeX * 4);
                        offsetInChild += form.SizeX;
                        offsetInThisElement += sizeX;
                    }
                } else {
                    uint offsetInThisElement = (uint)((form.RelativePosY * sizeX) + form.RelativePosX);  // Only updated per-line
                    uint offsetInChild = 0; // Only updated per-line

                    for (var y = 0; y < form.SizeY; y++) {
                        for (var x = 0; x < form.SizeX; x++) {
                            var childBufVal = form.Buffer[offsetInChild + x];
                            var childBufValAlpha = (byte)((childBufVal >> 24) & 0xFF);
                            var currentBufVal = Buffer[offsetInThisElement + x];

                            if (childBufValAlpha == 0) continue;

                            if (childBufValAlpha == 255) {
                                Buffer[offsetInThisElement + x] = childBufVal;
                            } else {
                                byte red = (byte)(((childBufVal >> 16) & 0xFF) * childBufValAlpha + ((currentBufVal >> 16) & 0xFF) * (255 - childBufValAlpha) >> 8);
                                byte green = (byte)(((childBufVal >> 8) & 0xFF) * childBufValAlpha + ((currentBufVal >> 8) & 0xFF) * (255 - childBufValAlpha) >> 8);
                                byte blue = (byte)(((childBufVal >> 0) & 0xFF) * childBufValAlpha + ((currentBufVal >> 0) & 0xFF) * (255 - childBufValAlpha) >> 8);
                                
                                Buffer[offsetInThisElement + x] = (uint)((0xFF << 24) + (red << 16) + (green << 8) + blue);
                            }
                        }

                        offsetInChild += form.SizeX;
                        offsetInThisElement += sizeX;
                    }
                }
                #endregion
            }

            Target.DrawBuffer(0, 0, sizeX, Buffer, BufferDrawMode.RawCopy);
        }

        public static void AddForm(Form form) {
            forms.Add(form);
        }

        public static void RemoveForm(Form form) {
            forms.Remove(form);
        }

        public static void PutToFront(Form form) {
            forms.Remove(form);
            forms.Add(form);
        }


    }

    public interface IRenderTarget {
        public void DrawBuffer(uint xStart, uint yStart, uint w, uint[] buffer, BufferDrawMode mode);
        public (uint w, uint h) GetSize();
    }

    public enum BufferDrawMode {
        /// <summary>
        /// Copies the buffer 1:1 to the target; not useful when transparency is required; generally considered fast as memory operations can speed this up heavily
        /// </summary>
        RawCopy,
        /// <summary>
        /// Copies each pixel individually; works with transparency; generally considered slow
        /// </summary>
        PixelByPixel,
    }

    /// <summary>
    /// Reference implementations: Not used to save on method call overhead on unoptimized non-jitted platforms
    /// </summary>
    public class ColorUtils {
        public byte GetA(uint color) {
            return (byte)((color >> 24) & 0xFF);
        }

        public byte GetR(uint color) {
            return (byte)((color >> 16) & 0xFF);
        }

        public byte GetG(uint color) {
            return (byte)((color >> 8) & 0xFF);
        }

        public byte GetB(uint color) {
            return (byte)((color) & 0xFF);
        }


    }
}
