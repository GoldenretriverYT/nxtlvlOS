﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Cosmos.Debug.Kernel;
using Cosmos.HAL.Drivers.Video.SVGAII;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using nxtlvlOS.Windowing;

namespace nxtlvlOS {
    /// <summary>
    /// SVGAIIScreen class. Used to draw ractengales to the screen. See also: <seealso cref="Canvas"/>.
    /// Based on Cosmos' builtin SVGAIICanvas class.
    /// </summary>
    public class spagSVGAII : Canvas {
        /// <summary>
        /// Debugger.
        /// </summary>
        internal Debugger mSVGAIIDebugger = new Debugger("SystemSVGAIIScreen");

        private static readonly Mode _DefaultMode = new Mode(1024, 768, ColorDepth.ColorDepth32);

        /// <summary>
        /// Graphics mode.
        /// </summary>
        private Mode _Mode;

        /// <summary>
        /// VMWare SVGA 2 driver.
        /// </summary>
        public readonly VMWareSVGAII _xSVGADriver;

        /// <summary>
        /// Create new instance of the <see cref="SVGAIICanvas"/> class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if default graphics mode is not suppoted.</exception>
        public spagSVGAII()
            : this(_DefaultMode) {
        }

        /// <summary>
        /// Create new instance of the <see cref="SVGAIICanvas"/> class.
        /// </summary>
        /// <param name="aMode">A graphics mode.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if mode is not suppoted.</exception>
        public spagSVGAII(Mode aMode) {
            mSVGAIIDebugger.SendInternal($"Called ctor with mode {aMode}");
            ThrowIfModeIsNotValid(aMode);

            _xSVGADriver = new VMWareSVGAII();
            Mode = aMode;
        }

        /// <summary>
        /// Name of the backend
        /// </summary>
        public override string Name() => "VMWareSVGAII";

        /// <summary>
        /// Get and set graphics mode.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">(set) Thrown if mode is not suppoted.</exception>
        public override Mode Mode {
            get => _Mode;
            set {
                _Mode = value;
                mSVGAIIDebugger.SendInternal($"Called Mode set property with mode {_Mode}");
                SetGraphicsMode(_Mode);
            }
        }


        /// <summary>
        /// Disables the SVGA driver, parent method returns to VGA text mode
        /// </summary>
        public override void Disable() {
            _xSVGADriver.Disable();
        }

        /// <summary>
        /// Draw array of colors.
        /// Not implemented.
        /// </summary>
        /// <param name="aColors">Array of colors.</param>
        /// <param name="aX">X coordinate.</param>
        /// <param name="aY">Y coordinate.</param>
        /// <param name="aWidth">Width.</param>
        /// <param name="aHeight">Height.</param>
        /// <exception cref="NotImplementedException">Thrown always.</exception>
        public override void DrawArray(Color[] aColors, int aX, int aY, int aWidth, int aHeight) {
            throw new NotImplementedException();
            //xSVGAIIDriver.
        }

        /// <summary>
        /// Draw filled rectangle.
        /// </summary>
        /// <param name="aColor">color to draw with.</param>
        /// <param name="aX_start">starting X coordinate.</param>
        /// <param name="aY_start">starting Y coordinate.</param>
        /// <param name="aWidth">Width.</param>
        /// <param name="aHeight">Height.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="NotImplementedException">Thrown if VMWare SVGA 2 has no rectange copy capability</exception>
        public override void DrawFilledRectangle(Color aColor, int aX_start, int aY_start, int aWidth, int aHeight) {
            DrawFilledRectangle(aColor.ToArgb(), aX_start, aY_start, aWidth, aHeight);
        }

        public void DrawFilledRectangle(int color, int aX_start, int aY_start, int aWidth, int aHeight) {
            for (int i = aY_start; i < aY_start + aHeight; i++) {
                _xSVGADriver.videoMemory.Fill((int)((aX_start * ((int)Mode.ColorDepth / 8) + i * (Mode.Width * ((int)Mode.ColorDepth / 8))) + (int)_xSVGADriver.FrameSize), aWidth, color);
            }
        }

        //public override IReadOnlyList<Mode> AvailableModes { get; } = new List<Mode>
        /// <summary>
        /// Available SVGA 2 supported video modes.
        /// <para>
        /// SD:
        /// <list type="bullet">
        /// <item>320x200x32.</item>
        /// <item>320x240x32.</item>
        /// <item>640x480x32.</item>
        /// <item>720x480x32.</item>
        /// <item>800x600x32.</item>
        /// <item>1024x768x32.</item>
        /// <item>1152x768x32.</item>
        /// </list>
        /// </para>
        /// <para>
        /// HD:
        /// <list type="bullet">
        /// <item>1280x720x32.</item>
        /// <item>1280x768x32.</item>
        /// <item>1280x800x32.</item>
        /// <item>1280x1024x32.</item>
        /// </list>
        /// </para>
        /// <para>
        /// HDR:
        /// <list type="bullet">
        /// <item>1360x768x32.</item>
        /// <item>1366x768x32.</item>
        /// <item>1440x900x32.</item>
        /// <item>1400x1050x32.</item>
        /// <item>1600x1200x32.</item>
        /// <item>1680x1050x32.</item>
        /// </list>
        /// </para>
        /// <para>
        /// HDTV:
        /// <list type="bullet">
        /// <item>1920x1080x32.</item>
        /// <item>1920x1200x32.</item>
        /// </list>
        /// </para>
        /// <para>
        /// 2K:
        /// <list type="bullet">
        /// <item>2048x1536x32.</item>
        /// <item>2560x1080x32.</item>
        /// <item>2560x1600x32.</item>
        /// <item>2560x2048x32.</item>
        /// <item>3200x2048x32.</item>
        /// <item>3200x2400x32.</item>
        /// <item>3840x2400x32.</item>
        /// </list>
        /// </para>
        /// </summary>
        public override List<Mode> AvailableModes { get; } = new List<Mode>
        {
        /*  VmWare maybe supports 16 bit resolutions but CGS not yet (we should need to do RGB32->RGB16 conversion) */
#if false
        /* 16-bit Depth Resolutions*/

        /* SD Resolutions */
        new Mode(320, 200, ColorDepth.ColorDepth16),
        new Mode(320, 240, ColorDepth.ColorDepth16),
        new Mode(640, 480, ColorDepth.ColorDepth16),
        new Mode(720, 480, ColorDepth.ColorDepth16),
        new Mode(800, 600, ColorDepth.ColorDepth16),
        new Mode(1024, 768, ColorDepth.ColorDepth16),
        new Mode(1152, 768, ColorDepth.ColorDepth16),

        /* Old HD-Ready Resolutions */
        new Mode(1280, 720, ColorDepth.ColorDepth16),
        new Mode(1280, 768, ColorDepth.ColorDepth16),
        new Mode(1280, 800, ColorDepth.ColorDepth16),  // WXGA
        new Mode(1280, 1024, ColorDepth.ColorDepth16), // SXGA

        /* Better HD-Ready Resolutions */
        new Mode(1360, 768, ColorDepth.ColorDepth16),
        new Mode(1366, 768, ColorDepth.ColorDepth16),  // Original Laptop Resolution
        new Mode(1440, 900, ColorDepth.ColorDepth16),  // WXGA+
        new Mode(1400, 1050, ColorDepth.ColorDepth16), // SXGA+
        new Mode(1600, 1200, ColorDepth.ColorDepth16), // UXGA
        new Mode(1680, 1050, ColorDepth.ColorDepth16), // WXGA++

        /* HDTV Resolutions */
        new Mode(1920, 1080, ColorDepth.ColorDepth16),
        new Mode(1920, 1200, ColorDepth.ColorDepth16), // WUXGA

        /* 2K Resolutions */
        new Mode(2048, 1536, ColorDepth.ColorDepth16), // QXGA
        new Mode(2560, 1080, ColorDepth.ColorDepth16), // UW-UXGA
        new Mode(2560, 1600, ColorDepth.ColorDepth16), // WQXGA
        new Mode(2560, 2048, ColorDepth.ColorDepth16), // QXGA+
        new Mode(3200, 2048, ColorDepth.ColorDepth16), // WQXGA+
        new Mode(3200, 2400, ColorDepth.ColorDepth16), // QUXGA
        new Mode(3840, 2400, ColorDepth.ColorDepth16), // WQUXGA
#endif
    /* 32-bit Depth Resolutions*/
    /* SD Resolutions */
    new Mode(320, 200, ColorDepth.ColorDepth32),
            new Mode(320, 240, ColorDepth.ColorDepth32),
            new Mode(640, 480, ColorDepth.ColorDepth32),
            new Mode(720, 480, ColorDepth.ColorDepth32),
            new Mode(800, 600, ColorDepth.ColorDepth32),
            new Mode(1024, 768, ColorDepth.ColorDepth32),
            new Mode(1152, 768, ColorDepth.ColorDepth32),

            /* Old HD-Ready Resolutions */
            new Mode(1280, 720, ColorDepth.ColorDepth32),
            new Mode(1280, 768, ColorDepth.ColorDepth32),
            new Mode(1280, 800, ColorDepth.ColorDepth32),  // WXGA
            new Mode(1280, 1024, ColorDepth.ColorDepth32), // SXGA

            /* Better HD-Ready Resolutions */
            new Mode(1360, 768, ColorDepth.ColorDepth32),
            //new Mode(1366, 768, ColorDepth.ColorDepth32),  // Original Laptop Resolution - this one is for some reason broken in vmware
            new Mode(1440, 900, ColorDepth.ColorDepth32),  // WXGA+
            new Mode(1400, 1050, ColorDepth.ColorDepth32), // SXGA+
            new Mode(1600, 1200, ColorDepth.ColorDepth32), // UXGA
            new Mode(1680, 1050, ColorDepth.ColorDepth32), // WXGA++

            /* HDTV Resolutions */
            new Mode(1920, 1080, ColorDepth.ColorDepth32),
            new Mode(1920, 1200, ColorDepth.ColorDepth32), // WUXGA

            /* 2K Resolutions */
            new Mode(2048, 1536, ColorDepth.ColorDepth32), // QXGA
            new Mode(2560, 1080, ColorDepth.ColorDepth32), // UW-UXGA
            new Mode(2560, 1600, ColorDepth.ColorDepth32), // WQXGA
            new Mode(2560, 2048, ColorDepth.ColorDepth32), // QXGA+
            new Mode(3200, 2048, ColorDepth.ColorDepth32), // WQXGA+
            new Mode(3200, 2400, ColorDepth.ColorDepth32), // QUXGA
            new Mode(3840, 2400, ColorDepth.ColorDepth32), // WQUXGA
    };

        public override Mode DefaultGraphicsMode => throw new NotImplementedException();

        /// <summary>
        /// Set graphics mode.
        /// </summary>
        /// <param name="aMode">A mode.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if mode is not suppoted.</exception>
        private void SetGraphicsMode(Mode aMode) {
            ThrowIfModeIsNotValid(aMode);

            var xWidth = (uint)aMode.Width;
            var xHeight = (uint)aMode.Height;
            var xColorDepth = (uint)aMode.ColorDepth;

            _xSVGADriver.SetMode(xWidth, xHeight, xColorDepth);
        }

        /// <summary>
        /// Clear screen to specified color.
        /// </summary>
        /// <param name="aColor">Color in ARGB.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="NotImplementedException">Thrown if VMWare SVGA 2 has no rectange copy capability</exception>
        public override void Clear(int aColor) {
            _xSVGADriver.Clear((uint)aColor);
        }

        /// <summary>
        /// Clear screen to specified color.
        /// </summary>
        /// <param name="aColor">Color.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="NotImplementedException">Thrown if VMWare SVGA 2 has no rectange copy capability</exception>
        public override void Clear(Color aColor) {
            _xSVGADriver.Clear((uint)aColor.ToArgb());
        }

        /// <summary>
        /// Get pixel color.
        /// </summary>
        /// <param name="aX">A X coordinate.</param>
        /// <param name="aY">A Y coordinate.</param>
        /// <returns>Color value.</returns>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public Color GetPixel(int aX, int aY) {
            var xColorARGB = _xSVGADriver.GetPixel((uint)aX, (uint)aY);
            return Color.FromArgb((int)xColorARGB);
        }

        /// <summary>
        /// Set cursor.
        /// </summary>
        /// <param name="aVisible">Visible.</param>
        /// <param name="aX">A X coordinate.</param>
        /// <param name="aY">A Y coordinate.</param>
        public void SetCursor(bool aVisible, int aX, int aY) {
            _xSVGADriver.SetCursor(aVisible, (uint)aX, (uint)aY);
        }

        /// <summary>
        /// Create cursor.
        /// </summary>
        public void CreateCursor() {
        }

        /// <summary>
        /// Copy pixel
        /// </summary>
        /// <param name="aX">A source X coordinate.</param>
        /// <param name="aY">A source Y coordinate.</param>
        /// <param name="aNewX">A destination X coordinate.</param>
        /// <param name="aNewY">A destination Y coordinate.</param>
        /// <param name="aWidth">A width.</param>
        /// <param name="aHeight">A height.</param>
        /// <exception cref="NotImplementedException">Thrown if VMWare SVGA 2 has no rectangle copy capability</exception>
        public void CopyPixel(int aX, int aY, int aNewX, int aNewY, int aWidth = 1, int aHeight = 1) {
            _xSVGADriver.Copy((uint)aX, (uint)aY, (uint)aNewX, (uint)aNewY, (uint)aWidth, (uint)aHeight);
        }

        /// <summary>
        /// Move pixel
        /// </summary>
        /// <param name="aX">A X coordinate.</param>
        /// <param name="aY">A Y coordinate.</param>
        /// <param name="aNewX">A new X coordinate.</param>
        /// <param name="aNewY">A new Y coordinate.</param>
        /// <exception cref="NotImplementedException">Thrown if VMWare SVGA 2 has no rectange copy capability</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public void MovePixel(int aX, int aY, int aNewX, int aNewY) {
            _xSVGADriver.Copy((uint)aX, (uint)aY, (uint)aNewX, (uint)aNewY, 1, 1);
            _xSVGADriver.SetPixel((uint)aX, (uint)aY, 0);
        }

        /// <summary>
        /// Display screen
        /// </summary>
        public override void Display() {
            _xSVGADriver.DoubleBufferUpdate();
        }

        /// <summary>
        /// Draw string.
        /// </summary>
        /// <param name="str">string to draw.</param>
        /// <param name="aFont">Font used.</param>
        /// <param name="color">Color.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public override void DrawString(string str, Font aFont, Color color, int x, int y) {
            for (int i = 0; i < str.Length; i++) {
                DrawChar(str[i], aFont, color, x, y);
                x += aFont.Width;
            }
        }

        /// <summary>
        /// Draw char.
        /// </summary>
        /// <param name="str">char to draw.</param>
        /// <param name="aFont">Font used.</param>
        /// <param name="color">Color.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public override void DrawChar(char c, Font aFont, Color color, int x, int y) {
            int p = aFont.Height * (byte)c;

            for (int cy = 0; cy < aFont.Height; cy++) {
                for (byte cx = 0; cx < aFont.Width; cx++) {
                    if (aFont.ConvertByteToBitAddress(aFont.Data[p + cy], cx + 1)) {
                        uint aX = (ushort)(x + (aFont.Width - cx));
                        uint aY = (ushort)(y + cy);
                        if (color.A < 255) {
                            if (color.A == 0) {
                                return;
                            }

                            color = AlphaBlend(color, Color.FromArgb((int)_xSVGADriver.GetPixel((uint)aX, (uint)aY)), color.A);
                        }

                        _xSVGADriver.SetPixel((uint)aX, (uint)aY, (uint)color.ToArgb());
                    }
                }
            }
        }

        /// <summary>
        /// Draw image.
        /// </summary>
        /// <param name="aImage">Image.</param>
        /// <param name="aX">X coordinate.</param>
        /// <param name="aY">Y coordinate.</param>
        public override void DrawImage(Image aImage, int aX, int aY) {
            var xWidth = (int)aImage.Width;
            var xHeight = (int)aImage.Height;
            int dataXWidth = ((int)Mode.ColorDepth / 8);

            for (int i = 0; i < xHeight; i++) {
                _xSVGADriver.videoMemory.Copy((int)((aX * dataXWidth + (aY + i) * (Mode.Width * (dataXWidth))) + (int)_xSVGADriver.FrameSize), aImage.RawData, i * xWidth, xWidth);
            }
        }

        /// <summary>
        /// Get point color.
        /// </summary>
        /// <param name="aX">X coordinate.</param>
        /// <param name="aY">Y coordinate.</param>
        /// <returns>Color value.</returns>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public override Color GetPointColor(int aX, int aY) {
            return Color.FromArgb((int)_xSVGADriver.GetPixel((uint)aX, (uint)aY));
        }

        public int GetPointColorRaw(int aX, int aY) {
            return (int)_xSVGADriver.GetPixel((uint)aX, (uint)aY);
        }

        /// <summary>
        /// Draw point.
        /// </summary>
        /// <param name="aColor">Color to draw with.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public override void DrawPoint(Color aColor, int aX, int aY) {
            if (aColor.A < 255) {
                if (aColor.A == 0) {
                    return;
                }

                aColor = AlphaBlend(aColor, GetPointColor(aX, aY), aColor.A);
            }

            _xSVGADriver.SetPixel((uint)aX, (uint)aY, (uint)aColor.ToArgb());
        }

        public void DrawImageFromIntArr(int[] array, int aX, int aY, int w, int h) {
            /*var xWidth = (int)w;
            var xHeight = (int)h;
            int dataXWidth = ((int)Mode.ColorDepth / 8);

            for (int i = 0; i < xHeight; i++) {
                _xSVGADriver.VideoMemory.Copy((aX * dataXWidth + (aY + i) * (Mode.Width * (dataXWidth))) + (int)_xSVGADriver.FrameSize, array, i * xWidth, xWidth);
            }*/

            for (int i = 0; i < h; i++) {
                for (int j = 0; j < w; j++) {
                    DrawPoint(Color.FromArgb(array[i * w + j]), aX + j, aY + i);
                }
            }
        }
    }
}