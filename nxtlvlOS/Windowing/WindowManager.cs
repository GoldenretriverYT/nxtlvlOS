using Cosmos.Core;
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
        public static List<Action> OverlayDrawers = new();
        private static uint wmSizeX, wmSizeY;

        private static MouseState previousState = MouseState.None;

        public static BufferedElement FocusedElement;
        public static BufferedElement PreviouslyHoveredElement = null;

        public static Event<(MouseState state, uint x, uint y)> GlobalMouseDownEvent = new();
        public static Event<(MouseState state, uint x, uint y)> GlobalMouseUpEvent = new();

        /// <summary>
        /// Activates debug utilities; can cause performance impact
        /// 
        /// Current features:
        ///   - Dump the hierarchy of the element under the mouse on click
        /// </summary>
        const bool DEBUG = false;

        /// <summary>
        /// The default font/the preferred font of the user.
        /// Setter may only be used with null to reset the cache.
        /// </summary>
        public static Font DefaultFont {
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

                try { 
                    if (preference.EndsWith(".psf"))
                        cachedPreferredFont = PCScreenFont.LoadFont(File.ReadAllBytes(preference));
                    else if (preference.EndsWith(".ttf"))
                        cachedPreferredFont = new TTFFont(File.ReadAllBytes(preference));
                    else
                        throw new Exception($"Unknown font file extension for {preference}.");
                }catch(Exception ex) {
                    SystemPreferenceService.Instance.SetPreference("wm.default_font", "system_default");
                    Kernel.Instance.Logger.Log(LogLevel.Fail, $"Failed to load font {preference}: {ex.Message}; Reverting to default!");
                    cachedPreferredFont = PCScreenFont.Default;
                    return cachedPreferredFont;
                }

                return cachedPreferredFont;
            }
            set {
                if(value != null) {
                    throw new ArgumentException("DefaultFont may only be set to null to reset the cache");
                }

                cachedPreferredFont = null;
            }
        }

        private static Font cachedPreferredFont = null;

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

            cursorElement.Image = cursorBmp;
            cursorElement.SetTransparent(true);
            cursorElement.Draw();

            MouseManager.ScreenWidth = wmSizeX;
            MouseManager.ScreenHeight = wmSizeY;

            if(!MouseManager.ScrollWheelPresent) {
                Kernel.Instance.Logger.Log(LogLevel.Warn, "Scroll wheel not present; scrolling will not work.");
            } else {
                Kernel.Instance.Logger.Log(LogLevel.Info, "Scroll wheel present!");
            }
        }

        public static WMResult Update() {
            try {
                // Clear buffer
                //System.Buffer.BlockCopy(EmptyBuffer, 0, Buffer, 0, Buffer.Length * 4);
                MemoryOperations.Fill(Buffer, 0xFF4CAACF);

                foreach (var form in Forms.ToList()) {
                    form.Update();
                }

                var elements = IEnumerableHelpers.FlattenElements(Forms);

                cursorElement.RelativePosX = (int)MouseManager.X;
                cursorElement.RelativePosY = (int)MouseManager.Y;
                elements.Add(cursorElement);

                // Get the element that is under the mouse
                BufferedElement elementUnderMouse = null;
                BufferedElement scrollTarget = null;

                foreach (var el in elements) {
                    var absolute = el.GetAbsolutePosition();

                    if(ShapeCollisions.RectIntersectsWithPoint(
                        absolute.y, absolute.x, absolute.y + el.SizeY, absolute.x + el.SizeX,
                        MouseManager.X, MouseManager.Y) && el.VisibleIncludingParents) {
                        if (!el.MousePassThrough) {
                            elementUnderMouse = el;
                        }

                        if(!el.ScrollPassThrough) {
                            scrollTarget = el;
                        }

                        continue;
                    }
                }

                if (PreviouslyHoveredElement != elementUnderMouse && elementUnderMouse != null) {
                    if (PreviouslyHoveredElement != null) {
                        //Kernel.Instance.Logger.Log(LogLevel.Sill, $"Element {PreviouslyHoveredElement.GetType().Name} ({PreviouslyHoveredElement.CustomId}): OnHoverEnd");
                        PreviouslyHoveredElement.OnHoverEnd(); // TODO: Crashes sometimes
                    }

                    PreviouslyHoveredElement = elementUnderMouse;
                    //Kernel.Instance.Logger.Log(LogLevel.Sill, $"Element {elementUnderMouse.GetType().Name} ({elementUnderMouse.CustomId}): OnHoverStart");
                    elementUnderMouse.OnHoverStart();
                }

                // Handle mouse events
                // TODO: Handle mouse events for all mouse buttons
                if (elementUnderMouse != null) {
                    if (MouseManager.MouseState != previousState) {
                        if(DEBUG) {
                            PrintHierarchy(elementUnderMouse, new List<string>());
                        }

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

                if (scrollTarget != null && MouseManager.ScrollDelta != 0) {
                    scrollTarget.OnMouseScroll(MouseManager.ScrollDelta);

                    MouseManager.ResetScrollDelta();
                }

                while(KeyboardManager.KeyAvailable) {
                    if(KeyboardManager.TryReadKey(out var key)) {
                        FocusedElement?.OnKey(key);
                    }
                }

                foreach (var el in elements) {
                    if (!el.VisibleIncludingParents || !el.ShouldBeDrawnToScreen || el.IsDeleted) {
                        if(el.IsDeleted) {
                            Kernel.Instance.Logger.Log(LogLevel.Warn, $"Element {el.GetType().Name} ({el.CustomId}) is deleted; ignoring");
                        }

                        continue;
                    }

                    #region Copy Buffer
                    var (absolutePosX, absolutePosY) = el.GetAbsolutePosition();
                    var (xMinParentBounds, xMaxParentBounds, yMinParentBounds, yMaxParentBounds) = el.GetParentBounds();

                    if(el.CustomId == "__dbg__") {
                        Kernel.Instance.Logger.Log(LogLevel.Info, $"Absolute position: {absolutePosX}, {absolutePosY}");
                        Kernel.Instance.Logger.Log(LogLevel.Info, $"Parent bounds: xMin({xMinParentBounds}), xMax({xMaxParentBounds}), yMin({yMinParentBounds}), yMax({yMaxParentBounds})");
                    }

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
                        int lengthToCopy = (endX - startX) * 4;

                        for (var y = startY; y < endY; y++) {
                            // Adjust the length to copy based on the overlapping region
                            System.Buffer.BlockCopy(el.Buffer, (int)offsetInChild * 4, Buffer, (int)offsetInWMBuffer * 4, lengthToCopy);
                            offsetInChild += el.SizeX;
                            offsetInWMBuffer += wmSizeX;
                        }
                    } else if (el.DrawMode == BufferDrawMode.PixelByPixel) {
                        // Calculate the overlapping region
                        int startX = (int)Math.Max(absolutePosX, xMinParentBounds);
                        int endX = (int)Math.Min(absolutePosX + el.SizeX, xMaxParentBounds);
                        int startY = (int)Math.Max(absolutePosY, yMinParentBounds);
                        int endY = (int)Math.Min(absolutePosY + el.SizeY, yMaxParentBounds);

                        for (var y = startY; y < endY; y++) {
                            uint offsetInThisElement = (uint)((y * wmSizeX) + startX);
                            uint offsetInChild = (uint)((y - absolutePosY) * el.SizeX + (startX - absolutePosX));

                            for (var x = startX; x < endX; x++) {
                                var childBufVal = el.Buffer[offsetInChild + (x - startX)];
                                var currentBufVal = Buffer[offsetInThisElement + (x - startX)];
                                var childBufValAlpha = (byte)((childBufVal >> 24) & 0xFF);
                                var adjustedChildBufValAlpha = 255 - childBufValAlpha;

                                if (childBufValAlpha == 0) continue;

                                if (childBufValAlpha == 255) {
                                    Buffer[offsetInThisElement + (x - startX)] = childBufVal;
                                } else {
                                    byte red = (byte)(((childBufVal >> 16) & 0xFF) * childBufValAlpha + ((currentBufVal >> 16) & 0xFF) * (adjustedChildBufValAlpha) >> 8);
                                    byte green = (byte)(((childBufVal >> 8) & 0xFF) * childBufValAlpha + ((currentBufVal >> 8) & 0xFF) * (adjustedChildBufValAlpha) >> 8);
                                    byte blue = (byte)(((childBufVal >> 0) & 0xFF) * childBufValAlpha + ((currentBufVal >> 0) & 0xFF) * (adjustedChildBufValAlpha) >> 8);

                                    Buffer[offsetInThisElement + (x - startX)] = (uint)((0xFF << 24) + (red << 16) + (green << 8) + blue);
                                }

                                //Buffer[offsetInThisElement + (x - startX)] = ColorUtils.AlphaBlend(childBufVal, currentBufVal);
                            }
                        }
                    }
                    #endregion
                }

                foreach (var drawer in OverlayDrawers) {
                    drawer.Invoke();
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

        /// <summary>
        /// Only use this if you know what you are doing! This might not function properly.
        /// </summary>
        /// <param name="el"></param>
        public static void AddElement(BufferedElement el) {
            Forms.Add(el);
        }

        public static void RemoveForm(Form form) {
            Forms.Remove(form);
        }

        public static void RemoveElement(BufferedElement el) {
            Forms.Remove(el);
        }

        public static void PutToFront(Form form) {
            Forms.Remove(form);
            Forms.Add(form);
        }

        public static void PutToFront(BufferedElement el) {
            Forms.Remove(el);
            Forms.Add(el);
        }

        /// <summary>
        /// Adds a drawer to the overlay. Be careful as these are redrawn every frame and therefore impact performance significantly.
        /// </summary>
        /// <param name="el"></param>
        public static void AddToOverlay(Action drawer) {
            OverlayDrawers.Add(drawer);
        }

        public static void RemoveFromOverlay(Action drawer) {
            OverlayDrawers.Remove(drawer);
        }

        static void PrintHierarchy(BufferedElement el, List<string> currentLines) {
            // add 2 spaces to each line
            var newLines = currentLines.Select(x => "  " + x).ToList();

            // add the current element
            newLines.Insert(0, @$"-> {el.GetType().Name} (Id={el.CustomId}, RelX={el.RelativePosX}, RelY={el.RelativePosY}, AbsX={el.GetAbsolutePosition().x}, AbsY={el.GetAbsolutePosition().y}, SizeX={el.SizeX}, SizeY={el.SizeY})");
        
            if(el.Parent != null) {
                PrintHierarchy(el.Parent, newLines);
            } else {
                foreach(var line in newLines) {
                    var finalLines = newLines.Select(x => x.Replace("->", "  ")).ToList();
                    finalLines.Insert(0, "-> WindowManager");

                    foreach(var l in finalLines) {
                        Kernel.Instance.Logger.Log(LogLevel.Sill, l);
                    }
                }
            }
        }

        #region Overlay Draw Utilities
        public static void DrawOverlayLineHorizontal(uint x1, uint x2, uint y, uint colorArgb) {
            if (x1 > x2) {
                (x1, x2) = (x2, x1);
            }

            for (var x = x1; x < x2; x++) {
                Buffer[(y * wmSizeX) + x] = colorArgb;
            }
        }

        public static void DrawOverlayLineVertical(uint y1, uint y2, uint x, uint colorArgb) {
            if (y1 > y2) {
                (y1, y2) = (y2, y1);
            }

            for (var y = y1; y < y2; y++) {
                Buffer[(y * wmSizeX) + x] = colorArgb;
            }
        }

        public static void DrawOverlayRect(uint x1, uint y1, uint x2, uint y2, uint colorArgb) {
            DrawOverlayLineHorizontal(x1, x2, y1, colorArgb);
            DrawOverlayLineVertical(y1, y2, x1, colorArgb);
            DrawOverlayLineHorizontal(x1, x2, y2 - 1, colorArgb);
            DrawOverlayLineVertical(y1, y2, x2 - 1, colorArgb);
        }

        public static unsafe void DrawOverlayRectFilled(uint x1, uint y1, uint x2, uint y2, uint colorArgb) {
            if (x1 > x2) {
                (x1, x2) = (x2, x1);
            }

            if (y1 > y2) {
                (y1, y2) = (y2, y1);
            }

            if (x1 == 0 && y1 == 0 && x2 == wmSizeX && y2 == wmSizeY) {
                MemoryOperations.Fill(Buffer, colorArgb);
            } else {
                fixed (uint* bufPtr = Buffer) {
                    for (var y = y1; y < y2; y++) {
                        MemoryOperations.Fill(bufPtr + (y * wmSizeX) + x1, colorArgb, (int)(x2 - x1));
                    }
                }
            }
        }
        #endregion
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
