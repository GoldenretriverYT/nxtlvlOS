﻿using Cosmos.Core;
using Cosmos.System;
using nxtlvlOS.Assets;
using nxtlvlOS.Loaders;
using nxtlvlOS.Services;
using nxtlvlOS.Windowing.Elements;
using nxtlvlOS.Windowing.Fonts;
using nxtlvlOS.Windowing.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing
{
    public static class WindowManager {
        public static IRenderTarget Target;
        public static uint[] Buffer;
        public static uint[] EmptyBuffer;

        public static (uint w, uint h) ScreenSize => (wmSizeX, wmSizeY);

        public static List<BufferedElement> Forms = new();
        private static uint wmSizeX, wmSizeY;

        private static MouseState previousState = MouseState.None;

        public static BufferedElement FocusedElement;
        public static BufferedElement PreviouslyHoveredElement = null;

        public static Event<(MouseState state, uint x, uint y)> GlobalMouseDownEvent = new();
        public static Event<(MouseState state, uint x, uint y)> GlobalMouseUpEvent = new();

        /// <summary>
        /// The default font/the preferred font of the user.
        /// Setter may only be used with null to reset the cache.
        /// </summary>
        public static PCScreenFont DefaultFont {
            get {
                if (cachedPreferredFont != null) return cachedPreferredFont;
                Kernel.Instance.Logger.Log(LogLevel.Sill, "Font not cached, loading default font.");
                
                var preference = SystemPreferenceService.Instance?.GetPreferenceOrDefault("wm.default_font", "system_default");
                Kernel.Instance.Logger.Log(LogLevel.Sill, $"Loading font {preference}...");
                if (preference == "system_default") {
                    cachedPreferredFont = PCScreenFont.Default;
                    return cachedPreferredFont;
                }else if (!File.Exists(preference)) {
                    SystemPreferenceService.Instance.SetPreference("wm.default_font", "system_default");
                    Kernel.Instance.Logger.Log(LogLevel.Warn, $"Font file {preference} does not exist, falling back to default font.");
                    cachedPreferredFont = PCScreenFont.Default;
                    return cachedPreferredFont;
                }

                cachedPreferredFont = PCScreenFont.LoadFont(File.ReadAllBytes(preference));

                return cachedPreferredFont;
            }
            set {
                if(value != null) {
                    throw new ArgumentException("DefaultFont may only be set to null to reset the cache");
                }

                cachedPreferredFont = null;
            }
        }

        private static PCScreenFont cachedPreferredFont = null;

        //private static BufferedElement currentHoveredElement;

        /// <summary>
        /// The mouse cursor icon; must be 24*24
        /// </summary>
        private static ImageLabel cursorElement = new() {
            SizeX = 24,
            SizeY = 24
        };

        public static void Init() {
            (wmSizeX, wmSizeY) = Target.GetSize();
            Buffer = new uint[wmSizeX * (wmSizeY+24)]; // Allocate 24 extra lines as a safe room for out of bound write
            EmptyBuffer = new uint[wmSizeX * (wmSizeY+24)];

            MemoryOperations.Fill(EmptyBuffer, 0xFF4CAACF);

            InitCursor();
        }

        public static void InitCursor() {
            NXTBmp cursorBmp = new(AssetManager.CursorBmp);

            cursorElement.SetImage(cursorBmp.Data);
            cursorElement.SetTransparent(true);
            cursorElement.Draw();

            MouseManager.ScreenWidth = wmSizeX;
            MouseManager.ScreenHeight = wmSizeY;
        }

        public static WMResult Update() {
            try {
                // Clear buffer
                System.Buffer.BlockCopy(EmptyBuffer, 0, Buffer, 0, Buffer.Length * 4);

                foreach (var form in Forms.ToList()) {
                    form.Update();
                }

                var elements = IEnumerableHelpers.FlattenElements(Forms);

                cursorElement.RelativePosX = (int)MouseManager.X;
                cursorElement.RelativePosY = (int)MouseManager.Y;
                elements.Add(cursorElement);

                // Get the element that is under the mouse
                BufferedElement elementUnderMouse = null;

                foreach (var el in elements) {
                    var absolute = el.GetAbsolutePosition();

                    if(ShapeCollisions.RectIntersectsWithPoint(
                        absolute.y, absolute.x, absolute.y + el.SizeY, absolute.x + el.SizeX,
                        MouseManager.X, MouseManager.Y) && el.VisibleIncludingParents && !el.MousePassThrough) {

                        elementUnderMouse = el;
                        continue;
                    }
                }

                if (PreviouslyHoveredElement != elementUnderMouse && elementUnderMouse != null) {
                    if (PreviouslyHoveredElement != null) {
                        //Kernel.Instance.Logger.Log(LogLevel.Sill, $"Element {PreviouslyHoveredElement.GetType().Name} ({PreviouslyHoveredElement.CustomId}): OnHoverEnd");
                        PreviouslyHoveredElement.OnHoverEnd();
                    }

                    PreviouslyHoveredElement = elementUnderMouse;
                    //Kernel.Instance.Logger.Log(LogLevel.Sill, $"Element {elementUnderMouse.GetType().Name} ({elementUnderMouse.CustomId}): OnHoverStart");
                    elementUnderMouse.OnHoverStart();
                }

                // Handle mouse events
                // TODO: Handle mouse events for all mouse buttons
                if (elementUnderMouse != null) {
                    if (MouseManager.MouseState != previousState) {
                        if ((MouseManager.MouseState & MouseState.Left) == MouseState.Left &&
                            (previousState & MouseState.Left) != MouseState.Left) {
                            FocusedElement = elementUnderMouse;

                            GlobalMouseDownEvent.Invoke((MouseManager.MouseState, MouseManager.X, MouseManager.Y));
                            elementUnderMouse.OnMouseDown(MouseManager.MouseState);
                        }else if ((MouseManager.MouseState & MouseState.Left) != MouseState.Left &&
                            (previousState & MouseState.Left) == MouseState.Left) {
                            GlobalMouseUpEvent.Invoke((MouseManager.MouseState, MouseManager.X, MouseManager.Y));
                            FocusedElement?.OnMouseUp(MouseManager.MouseState, previousState, elementUnderMouse == FocusedElement);
                        }

                        if ((MouseManager.MouseState & MouseState.Right) == MouseState.Right &&
                            (previousState & MouseState.Right) != MouseState.Right) {
                            FocusedElement = elementUnderMouse;
                            elementUnderMouse.OnMouseDown(MouseManager.MouseState);
                        } else if ((MouseManager.MouseState & MouseState.Right) != MouseState.Right &&
                            (previousState & MouseState.Right) == MouseState.Right) {

                            FocusedElement?.OnMouseUp(MouseManager.MouseState, previousState, elementUnderMouse == FocusedElement);
                        }

                        previousState = MouseManager.MouseState;
                    }
                }

                while(KeyboardManager.KeyAvailable) {
                    if(KeyboardManager.TryReadKey(out var key)) {
                        FocusedElement?.OnKey(key);
                    }
                }

                foreach (var el in elements) {
                    if (!el.VisibleIncludingParents || !el.ShouldBeDrawnToScreen || el.IsDeleted) continue;

                    #region Copy Buffer
                    var (absolutePosX, absolutePosY) = el.GetAbsolutePosition();
                    var (xMinParentBounds, xMaxParentBounds, yMinParentBounds, yMaxParentBounds) = el.GetParentBounds();

                    if (el.DrawMode == BufferDrawMode.RawCopy) {
                        // Calculate the overlapping region
                        int startX = (int)Math.Max(absolutePosX, xMinParentBounds);
                        int endX = (int)Math.Min(absolutePosX + el.SizeX, xMaxParentBounds);
                        int startY = (int)Math.Max(absolutePosY, yMinParentBounds);
                        int endY = (int)Math.Min(absolutePosY + el.SizeY, yMaxParentBounds);

                        // Offset in the window manager buffer
                        uint offsetInWMBuffer = (uint)(startY * wmSizeX + startX);
                        // Offset in the child buffer
                        uint offsetInChild = (uint)((startY - absolutePosY) * el.SizeX + (startX - absolutePosX));

                        if (endX - startX <= 0 || endY - startY <= 0) continue;

                        for (var y = startY; y < endY; y++) {
                            // Adjust the length to copy based on the overlapping region
                            int lengthToCopy = (endX - startX) * 4;
                            System.Buffer.BlockCopy(el.Buffer, (int)offsetInChild * 4, Buffer, (int)offsetInWMBuffer * 4, lengthToCopy);
                            offsetInChild += el.SizeX;
                            offsetInWMBuffer += wmSizeX;
                        }
                    } else {
                        uint offsetInThisElement = (uint)((absolutePosY * wmSizeX) + absolutePosX);  // Only updated per-line
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
                            offsetInThisElement += wmSizeX;
                        }
                    }
                    #endregion
                }

                Target.DrawBuffer(Buffer);
                //Thread.Sleep(100); // Slow down for testing

                return new() { Type = WMResultType.OK, AdditionalData = null };
            }catch(Exception ex) {
                return new() { Type = WMResultType.Failure, AdditionalData = $"Failed to update; partial updates may have occurred and corruption may have been caused by that. Error: {ex.Message}" };
            }
        }

        public static void AddForm(Form form) {
            Forms.Add(form);
        }

        public static void RemoveForm(Form form) {
            Forms.Remove(form);
        }

        public static void PutToFront(Form form) {
            Forms.Remove(form);
            Forms.Add(form);
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
        public void DrawBuffer(uint[] buffer);
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
