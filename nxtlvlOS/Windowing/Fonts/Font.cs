﻿using nxtlvlOS.Windowing.Elements;
using nxtlvlOS.Windowing.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace nxtlvlOS.Windowing.Fonts {
    /// <summary>
    /// Represents a bitmap font.
    /// </summary>
    public abstract class Font : ICopyable<Font> {
        /// <summary>
        /// Gets the raw pixel data of the bitmap font.
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// The height of a single character in pixels, or 0 if the font is variable height.
        /// </summary>
        public byte Height { get; }

        /// <summary>
        /// The width of a single character in pixels, or 0 if the font is variable width.
        /// </summary>
        public byte Width { get; }

        /// <summary>
        /// Converts a byte to its byte address.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConvertByteToBitAddress(byte byteToConvert, int bitToReturn) {
            return (byteToConvert & (1 << (bitToReturn - 1))) != 0;
        }

        public virtual (uint w, uint h) MeasureString(string str) {
            return ((uint)(str.Length * Width), Height);
        }

        public virtual (uint w, uint h) MeasureStringExhaustive(string str) {
            string[] lines = str.Split('\n');

            int wMax = 0;
            uint height = (uint)(lines.Length * Height);

            foreach(var line in lines) {
                var lineWidth = line.Length * Width;
                if (lineWidth > wMax) wMax = lineWidth;
            }

            return ((uint)wMax, height);
        }

        public (uint x, uint y) AlignWithin(string str, HorizontalAlignment horizontal, VerticalAlignment vertical, uint xBound, uint yBound) {
            uint resX = 0;
            uint resY = 0;

            var size = MeasureStringExhaustive(str);

            if(!(size.w > xBound)) {
                if(horizontal == HorizontalAlignment.Center) {
                    resX = (xBound - size.w) / 2;
                }else if(horizontal == HorizontalAlignment.Right) {
                    resX = (xBound - size.w);
                }
            }

            if(!(size.h > yBound)) {
                if(vertical == VerticalAlignment.Middle) {
                    resY = (yBound - size.h) / 2;
                } else if (vertical == VerticalAlignment.Bottom) {
                    resY = (yBound - size.h);
                }
            }

            return (resX, resY);
        }

        public abstract void DrawChar(BufferedElement el, int x, int y, char c, uint color, bool safe, bool dbg, bool internalAlphaBlend = false);

        /// <summary>
        /// Initializes a new instance of the <see cref="Font"/> class.
        /// </summary>
        /// <param name="width">The width of a single character in pixels</param>
        /// <param name="height">The height of a single character in pixels</param>
        /// <param name="data">The raw pixel data.</param>
        public Font(byte width, byte height, byte[] data) {
            Width = width;
            Height = height;
            Data = data;
        }

        public virtual int GetLineHeight() {
            return Height;
        }

        public virtual (int width, int lsb) GetGlyphMetrics(char c) {
            return (Width, 0);
        }

        public abstract Font Copy();
    }
}