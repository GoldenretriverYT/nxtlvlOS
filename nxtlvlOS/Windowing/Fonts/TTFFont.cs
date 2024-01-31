using CosmosTTF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Fonts {
    public class TTFFont : Font {
        private string _randomName;
        public int Size { get; set; } = 16;
        
        public TTFFont(byte[] data) : base(0, 0, data) {
            TTFManager.GlyphCacheSize = 128; // cache at most 128 glyphs
            _randomName = "font" + new Random().Next(0, 1000000);
            TTFManager.RegisterFont(_randomName, data);
        }

        private TTFFont() : base(0, 0, null) { }

        public override Font Copy() {
            return new TTFFont() {
                _randomName = _randomName,
                Size = Size
            };
        }

        public override void DrawChar(BufferedElement el, int x, int y, char c, uint color, bool safe, bool dbg, bool internalAlphaBlend = false) {
            try {
                var rendered = TTFManager.RenderGlyphAsBitmap(_randomName, c, Color.FromArgb((int)color), Size);
                //Kernel.Instance.Logger.Log(LogLevel.Sill, $"Rendered glyph {c} ({(int)c}) in font {_randomName} with size {Size} and color {color} to {rendered.bmp.Width}x{rendered.bmp.Height} bitmap, offY {rendered.offY}");
                // Its always safe due to the nature of DrawCosmosBitmap
                if (internalAlphaBlend) {
                    el.DrawCosmosBitmapAlpha(x, (int)(y + (Size * 0.8f) + rendered.offY), rendered.bmp);
                } else {
                    el.DrawCosmosBitmap(x, (int)(y + (Size*0.8f) + rendered.offY), rendered.bmp);
                }
            } catch (Exception e) {
                Kernel.Instance.Logger.Log(LogLevel.Error, $"Failed to render glyph {c} ({(int)c}) in font {_randomName}: {e.Message}");
            }
        }

        public override int GetLineHeight() {
            return Size; // we just assume that the line height is the font size - not always the case, but it aint getting any better
        }

        public override (int width, int lsb) GetGlyphMetrics(char c) {
            TTFManager.GetGlyphHMetrics(_randomName, c, Size, out var advanceWidth, out var lsb);
            return (advanceWidth, lsb);
        }

        public override (uint w, uint h) MeasureString(string str) {
            var w = TTFManager.GetTTFWidth(_randomName, str, Size);
            var h = Size;

            return ((uint)w, (uint)h);
        }

        public override (uint w, uint h) MeasureStringExhaustive(string str) {
            var lines = str.Split('\n');
            var wMax = 0;
            var h = lines.Length * Size;

            foreach(var line in lines) {
                var lineWidth = TTFManager.GetTTFWidth(line, _randomName, Size);
                if (lineWidth > wMax) wMax = lineWidth;
            }

            return ((uint)wMax, (uint)h);
        }
    }
}
