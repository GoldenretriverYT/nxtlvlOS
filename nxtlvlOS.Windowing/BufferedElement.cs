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

        public uint RelativePosX, RelativePosY;
        public uint SizeX = 100, SizeY = 100;
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

        public virtual void Update() {
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
                
                if(child.BufferWasUpdated) {
                    child.BufferWasUpdated = false;

                    #region Copy Buffer
                    if (child.DrawMode == BufferDrawMode.RawCopy) {
                        uint offsetInThisElement = (child.RelativePosY * SizeX) + child.RelativePosX;
                        uint offsetInChild = 0;

                        for (var y = 0; y < child.SizeY; y++) {
                            System.Buffer.BlockCopy(child.Buffer, (int)offsetInChild * 4, Buffer, (int)offsetInThisElement * 4, (int)child.SizeX * 4);
                            offsetInChild += child.SizeX;
                            offsetInThisElement += SizeX;
                        }
                    }else {
                        uint offsetInThisElement = (child.RelativePosY * SizeX) + child.RelativePosX;  // Only updated per-line
                        uint offsetInChild = 0; // Only updated per-line

                        for (var y = 0; y < child.SizeY; y++) {
                            for (var x = 0; x < child.SizeX; x++) {
                                var childBufVal = child.Buffer[offsetInChild + x];
                                var childBufValAlpha = (byte)((childBufVal >> 24) & 0xFF);

                                if (childBufValAlpha == 0) continue;

                                if (childBufValAlpha == 255) {
                                    Buffer[offsetInThisElement + x] = childBufVal;
                                }else {
                                    Buffer[offsetInThisElement + x] = 0xFFFF0000; // todo: add alpha blend
                                }
                            }

                            offsetInChild += child.SizeX;
                            offsetInThisElement += SizeX;
                        }
                    }
                    #endregion
                }
            }
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

        public (uint x, uint y) GetAbsolutePosition() {
            uint x = RelativePosX;
            uint y = RelativePosY;

            BufferedElement par = Parent;

            while(par != null) {
                x += par.RelativePosX;
                y += par.RelativePosY;

                par = par.Parent;
            }

            return (x, y);
        }
    }
}
