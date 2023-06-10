using nxtlvlOS.Windowing.Fonts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing {
    public abstract class BufferedElement {
        public BufferedElement Parent;
        public List<BufferedElement> Children = new();

        public int RelativePosX, RelativePosY;
        public uint SizeX = 100, SizeY = 100;
        public bool Visible = true;
        public uint[] Buffer;
        private uint _bufSizeX = 0, _bufSizeY = 0;

        protected bool dirty = false;

        public BufferDrawMode DrawMode = BufferDrawMode.RawCopy;

        /// <summary>
        /// If the buffer was updated since the last time this was set to false.
        /// This will be used as follows:
        ///   1) Element.Update
        ///   2) WindowManager/ParentElement checks if this is true
        ///   3) if it is, copies buffer to its own buffer
        ///      or if its not, continues with the next element
        /// </summary>
        public bool BufferWasUpdated = true;

        public Action PreDrawAndChildUpdate = () => { };
        public Action PostDrawAndChildUpdate = () => { };

        public virtual void Update() {
            PreDrawAndChildUpdate();

            if (_bufSizeX != SizeX || _bufSizeY != SizeY) { // TODO: Implement copy of all children buffers
                _bufSizeX = SizeX;
                _bufSizeY = SizeY;

                Buffer = new uint[SizeY * SizeX];
            }


            if(dirty) {
                Draw();
                BufferWasUpdated = true;
            }


            foreach(var child in Children) {
                child.Update();
            }

            PostDrawAndChildUpdate();
        }

        public void SetDirty(bool isDirty) {
            this.dirty = isDirty;
        }

        public abstract void Draw();

        public void Clear(uint colorArgb = 0x00000000) {
            for(var x = 0; x < SizeX; x++) {
                for(var y = 0; y < SizeY; y++) {
                    Buffer[(y * SizeX) + x] = colorArgb;
                }
            }
        }

        public void DrawLineHorizontal(uint x1, uint x2, uint y, uint colorArgb) {
            if (x1 > x2) {
                (x1, x2) = (x2, x1);
            }

            for (var x = x1; x < x2; x++) {
                Buffer[(y * SizeX) + x] = colorArgb;
            }
        }

        public void DrawLineVertical(uint y1, uint y2, uint x, uint colorArgb) {
            if (y1 > y2) {
                (y1, y2) = (y2, y1);
            }

            for (var y = y1; y < y2; y++) {
                Buffer[(y * SizeX) + x] = colorArgb;
            }
        }

        public void DrawRect(uint x1, uint y1, uint x2, uint y2, uint colorArgb) {
            DrawLineHorizontal(x1, x2, y1, colorArgb);
            DrawLineVertical(y1, y2, x1, colorArgb);
            DrawLineHorizontal(x1, x2, y2, colorArgb);
            DrawLineVertical(y1, y2, x2, colorArgb);
        }

        public void DrawRectFilled(uint x1, uint y1, uint x2, uint y2, uint colorArgb) {
            if (x1 > x2) {
                (x1, x2) = (x2, x1);
            }

            if (y1 > y2) {
                (y1, y2) = (y2, y1);
            }

            for (var x = x1; x < x2; x++) {
                for (var y = y1; y < y2; y++) {
                    Buffer[(y * SizeX) + x] = colorArgb;
                }
            }
        }

        public void DrawInsetRectFilled(uint x1, uint y1, uint x2, uint y2, uint colorArgb, uint insetColorArgb) {
            if (x1 > x2) {
                (x1, x2) = (x2, x1);
            }

            if (y1 > y2) {
                (y1, y2) = (y2, y1);
            }

            for (var x = x1; x < x2; x++) {
                for (var y = y1; y < y2; y++) {
                    Buffer[(y * SizeX) + x] = colorArgb;
                }
            }

            DrawRectFilled(x1, y1, x1+2, y2, 0xFFFFFFFF);
            DrawRectFilled(x1, y1, x2, y1+2, 0xFFFFFFFF);

            DrawRectFilled(x2 - 2, 0, x2, y2, insetColorArgb);
            DrawRectFilled(x1, y2 - 2, x2, y2, insetColorArgb);
        }

        public void DrawStringPSF(PCScreenFont font, uint x, uint y, string str, uint color) {
            var xOffset = x;

            foreach(var c in str) {
                DrawCharPSF(font, xOffset, y, c, color);
                xOffset += font.Width;
            }
        }

        public void DrawCharPSF(PCScreenFont font, uint x, uint y, char c, uint color) {
            int p = font.Height * (byte)c;

            for (int cy = 0; cy < font.Height; cy++) {
                for (byte cx = 0; cx < font.Width; cx++) {
                    if ((font.Data[p + cy] & (1 << (cx))) != 0) {
                        var xOff = x + (font.Width - cx);
                        var yOff = y + cy;

                        Buffer[(yOff * SizeX) + xOff] = color;
                    }
                }
            }
        }

        public (uint x, uint y) GetAbsolutePosition() {
            uint x = (uint)RelativePosX;
            uint y = (uint)RelativePosY;

            BufferedElement par = Parent;

            while(par != null) {
                x += (uint)par.RelativePosX;
                y += (uint)par.RelativePosY;

                par = par.Parent;
            }

            return (x, y);
        }
    }
}
