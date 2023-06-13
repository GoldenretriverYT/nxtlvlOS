using Cosmos.System;
using nxtlvlOS.Assets;
using nxtlvlOS.Loaders;
using nxtlvlOS.Windowing.Elements;
using nxtlvlOS.Windowing.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing
{
    public static class WindowManager {
        public static IRenderTarget Target;
        public static uint[] Buffer;
        public static uint[] EmptyBuffer;

        public static (uint w, uint h) ScreenSize => (sizeX, sizeY);

        private static List<BufferedElement> forms = new();
        private static uint sizeX, sizeY;

        private static MouseState previousState = MouseState.None;

        private static BufferedElement currentlyFocusedElement;

        //private static BufferedElement currentHoveredElement;

        /// <summary>
        /// The mouse cursor icon; must be 24*24
        /// </summary>
        private static ImageLabel cursorElement = new() {
            SizeX = 24,
            SizeY = 24
        };

        public static void Init() {
            (sizeX, sizeY) = Target.GetSize();
            Buffer = new uint[sizeX * (sizeY+24)]; // Allocate 24 extra lines for stuff overflowing, like the cursor
            EmptyBuffer = new uint[sizeX * (sizeY+24)];

            InitCursor();
        }

        public static void InitCursor() {
            NXTBmp cursorBmp = new(AssetManager.CursorBmp);

            cursorElement.SetImage(cursorBmp.Data);
            cursorElement.SetTransparent(true);
            cursorElement.Draw();

            MouseManager.ScreenWidth = sizeX;
            MouseManager.ScreenHeight = sizeY;
        }

        public static WMResult Update() {
            try {
                // Clear buffer
                System.Buffer.BlockCopy(EmptyBuffer, 0, Buffer, 0, Buffer.Length * 4);

                foreach (var form in forms) {
                    form.Update();
                }

                var elements = IEnumerableHelpers.Flatten(forms);

                cursorElement.RelativePosX = (int)MouseManager.X;
                cursorElement.RelativePosY = (int)MouseManager.Y;
                elements.Add(cursorElement);

                // mouse stuff
                BufferedElement elementUnderMouse = null;

                foreach (var el in elements) {
                    var absolute = el.GetAbsolutePosition();

                    if (MouseManager.X > absolute.x && MouseManager.X < absolute.x + el.SizeX &&
                        MouseManager.Y > absolute.y && MouseManager.Y < absolute.y + el.SizeY) {
                        elementUnderMouse = el;
                        continue;
                    }
                }

                if (elementUnderMouse != null) {
                    if (MouseManager.MouseState != previousState) {
                        if ((MouseManager.MouseState & MouseState.Left) == MouseState.Left &&
                            (previousState & MouseState.Left) != MouseState.Left) {
                            elementUnderMouse.OnMouseDown(MouseManager.MouseState);
                            currentlyFocusedElement = elementUnderMouse;
                        }else if ((MouseManager.MouseState & MouseState.Left) != MouseState.Left &&
                            (previousState & MouseState.Left) == MouseState.Left) {

                            currentlyFocusedElement.OnMouseUp(MouseManager.MouseState, elementUnderMouse == currentlyFocusedElement);
                            currentlyFocusedElement = null;
                        }

                        previousState = MouseManager.MouseState;
                    }
                }

                foreach(var el in elements) {
                    var absolute = el.GetAbsolutePosition();

                    if (MouseManager.X > absolute.x && MouseManager.X < absolute.x + el.SizeX &&
                        MouseManager.Y > absolute.y && MouseManager.Y < absolute.y + el.SizeY) {

                        continue;
                    }
                }

                foreach (var el in elements) {
                    if (!el.Visible) continue;

                    #region Copy Buffer
                    var (absolutePosX, absolutePosY) = el.GetAbsolutePosition();

                    if (el.DrawMode == BufferDrawMode.RawCopy) {
                        uint offsetInThisElement = (uint)((absolutePosY * sizeX) + absolutePosX);
                        uint offsetInChild = 0;

                        for (var y = 0; y < el.SizeY; y++) {
                            System.Buffer.BlockCopy(el.Buffer, (int)offsetInChild * 4, Buffer, (int)offsetInThisElement * 4, (int)el.SizeX * 4);
                            offsetInChild += el.SizeX;
                            offsetInThisElement += sizeX;
                        }
                    } else {
                        uint offsetInThisElement = (uint)((absolutePosY * sizeX) + absolutePosX);  // Only updated per-line
                        uint offsetInChild = 0; // Only updated per-line

                        for (var y = 0; y < el.SizeY; y++) {
                            for (var x = 0; x < el.SizeX; x++) {
                                var childBufVal = el.Buffer[offsetInChild + x];
                                var currentBufVal = Buffer[offsetInThisElement + x];
                                var childBufValAlpha = (byte)((childBufVal >> 24) & 0xFF);

                                if (childBufValAlpha == 0) continue;

                                if (childBufValAlpha == 255) {
                                    Buffer[offsetInThisElement + x] = childBufVal;
                                } else {
                                    byte red = (byte)(((childBufVal >> 16) & 0xFF) * childBufValAlpha + ((currentBufVal >> 16) & 0xFF) * (255 - childBufValAlpha) >> 8);
                                    byte green = (byte)(((childBufVal >> 8) & 0xFF) * childBufValAlpha + ((currentBufVal >> 8) & 0xFF) * (255 - childBufValAlpha) >> 8);
                                    byte blue = (byte)(((childBufVal >> 0) & 0xFF) * childBufValAlpha + ((currentBufVal >> 0) & 0xFF) * (255 - childBufValAlpha) >> 8);

                                    Buffer[offsetInThisElement + x] = (uint)((0xFF << 24) + (red << 16) + (green << 8) + blue);
                                }

                                //Buffer[offsetInThisElement + x] = ColorUtils.AlphaBlend(childBufVal, currentBufVal);
                            }

                            offsetInChild += el.SizeX;
                            offsetInThisElement += sizeX;
                        }
                    }
                    #endregion
                }

                Target.DrawBuffer(0, 0, sizeX, Buffer, BufferDrawMode.RawCopy);

                return new() { Type = WMResultType.OK, AdditionalData = null };
            }catch(Exception ex) {
                return new() { Type = WMResultType.Failure, AdditionalData = $"Failed to update; partial updates may have occurred and corruption may have been caused by that. Error: {ex.Message}" };
            }
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

    public struct WMResult {
        public WMResultType Type;
        public object AdditionalData;
    }

    public enum WMResultType {
        Failure,
        OK
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
    public class ColorUtilsA {
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
