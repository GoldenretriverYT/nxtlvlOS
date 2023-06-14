using Cosmos.System;
using nxtlvlOS.Windowing.Elements;
using nxtlvlOS.Windowing.Fonts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing {
    public abstract class BufferedElement {
        /// <summary>
        /// The parent of the element; may be null if it is a root element
        /// </summary>
        public BufferedElement Parent;
        /// <summary>
        /// The children of the element
        /// </summary>
        public List<BufferedElement> Children = new();

        /// <summary>
        /// The position of the element
        /// </summary>
        public int RelativePosX, RelativePosY;
        /// <summary>
        /// The size of the element - changing this will require a full redraw (automtically handled)
        /// </summary>
        public uint SizeX = 100, SizeY = 100;
        /// <summary>
        /// Whether or not the element should be visible
        /// </summary>
        public bool Visible = true;
        /// <summary>
        /// The drawing buffer of the element
        /// </summary>
        public uint[] Buffer = new uint[0];
        /// <summary>
        /// Prevents mouse events from bringing this element to the front
        /// </summary>
        public bool DoNotBringToFront = false;
        
        private uint _bufSizeX = 0, _bufSizeY = 0;

        protected bool dirty = false;

        /// <summary>
        /// The draw mode of the element; look at the enums values csdocs for more information.
        /// </summary>
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

        /// <summary>
        /// This event gets called before the element gets drawn and the childs updates will be called.
        /// </summary>
        public Action PreDrawAndChildUpdate = () => { };
        /// <summary>
        /// This event gets called after the element was drawn and the child updates were executed.
        /// </summary>
        public Action PostDrawAndChildUpdate = () => { };

        /// <summary>
        /// This event gets called when the element size changes.
        /// </summary>
        public Action SizeChanged = () => { };
        /// <summary>
        /// This event gets called when the element changes visibility.
        /// </summary>
        public Action VisibilityChanged = () => { };

        /// <summary>
        /// This even gets called when the OnMouseDown method is called. Elements can decide to not fire this event.
        /// </summary>
        public Action<MouseState, uint, uint> MouseDown = (MouseState state, uint absoluteX, uint absoluteY) => {
        };

        /// <summary>
        /// This even gets called when the OnMouseUp method is called. Elements can decide to not fire this event.
        /// </summary>
        public Action<MouseState, uint, uint> MouseUp = (MouseState state, uint absoluteX, uint absoluteY) => {
        };

        /// <summary>
        /// Whether or not the element is visible in the total point of view. If this or any parent is hidden, this is false.
        /// </summary>
        public bool VisibleIncludingParents => Visible && (Parent == null ? true : Parent.VisibleIncludingParents);
        public bool PreviousVisibility = false;

        /// <summary>
        /// Called every frame. Should not do any drawing.
        /// </summary>
        public virtual void Update() {
            PreDrawAndChildUpdate();

            if (_bufSizeX != SizeX || _bufSizeY != SizeY) {
                _bufSizeX = SizeX;
                _bufSizeY = SizeY;

                Buffer = new uint[SizeY * SizeX];
                SetDirty(true);
                SizeChanged();
            }

            if(PreviousVisibility != VisibleIncludingParents) {
                PreviousVisibility = VisibleIncludingParents;
                VisibilityChanged();
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

        public virtual void OnMouseDown(MouseState state) {
            this.BringToFront();

            MouseDown(state, MouseManager.X, MouseManager.Y);
        }

        public virtual void OnMouseUp(MouseState state, bool mouseIsOver) {
            MouseUp(state, MouseManager.X, MouseManager.Y);
        }

        public virtual void OnHoverStart() {
            // TODO: Implement this
        }

        public virtual void OnHoverEnd() {
            // TODO: Implement this
        }

        public virtual void OnKey(KeyEvent ev) {

        }

        /// <summary>
        /// Brings this element to the front of all siblings
        /// </summary>
        public void BringToFront() {
            if (DoNotBringToFront) return;

            if(Parent != null) {
                Parent.Children.Remove(this);
                Parent.Children.Add(this);

                Parent.BringToFront();
            }else if(this is Form f) { // a form & no parent? should be a root-level form
                WindowManager.PutToFront(f);
            }
        }

        /// <summary>
        /// Marks the elements buffer as dirty/not dirty. If marked as dirty it will cause a redraw.
        /// </summary>
        /// <param name="isDirty"></param>
        public void SetDirty(bool isDirty) {
            this.dirty = isDirty;
        }

        /// <summary>
        /// Adds a new child element to the element.
        /// </summary>
        /// <param name="el"></param>
        public void AddElement(BufferedElement el) {
            el.Parent = this;
            Children.Add(el);
        }

        /// <summary>
        /// Removes a child element from the element. 
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public bool RemoveElement(BufferedElement el) {
            if(Children.Contains(el)) {
                Children.Remove(el);
                el.Parent = null;
                return true;
            }

            return false;
        }

        public abstract void Draw();

        /// <summary>
        /// Converts the absolute position to a position relative to the top left corner of this element
        /// </summary>
        /// <param name="absoluteX"></param>
        /// <param name="absoluteY"></param>
        /// <returns></returns>
        public (int x, int y) GetRelativeFromAbsolutePosition(int absoluteX, int absoluteY) {
            var thisAbsolute = GetAbsolutePosition();
            absoluteX -= (int)thisAbsolute.x;
            absoluteY -= (int)thisAbsolute.y;

            return (absoluteX, absoluteY);
        }

        /// <summary>
        /// Clears the elements buffer
        /// </summary>
        /// <param name="colorArgb"></param>
        public void Clear(uint colorArgb = 0x00000000) {
            for(var x = 0; x < SizeX; x++) {
                for(var y = 0; y < SizeY; y++) {
                    Buffer[(y * SizeX) + x] = colorArgb;
                }
            }
        }

        /// <summary>
        /// Draws a horizontal line
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y"></param>
        /// <param name="colorArgb"></param>
        public void DrawLineHorizontal(uint x1, uint x2, uint y, uint colorArgb) {
            if (x1 > x2) {
                (x1, x2) = (x2, x1);
            }

            for (var x = x1; x < x2; x++) {
                Buffer[(y * SizeX) + x] = colorArgb;
            }
        }

        /// <summary>
        /// Draws a vertical line
        /// </summary>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <param name="x"></param>
        /// <param name="colorArgb"></param>
        public void DrawLineVertical(uint y1, uint y2, uint x, uint colorArgb) {
            if (y1 > y2) {
                (y1, y2) = (y2, y1);
            }

            for (var y = y1; y < y2; y++) {
                Buffer[(y * SizeX) + x] = colorArgb;
            }
        }

        /// <summary>
        /// Draws a normal rect
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="colorArgb"></param>
        public void DrawRect(uint x1, uint y1, uint x2, uint y2, uint colorArgb) {
            DrawLineHorizontal(x1, x2, y1, colorArgb);
            DrawLineVertical(y1, y2, x1, colorArgb);
            DrawLineHorizontal(x1, x2, y2-1, colorArgb);
            DrawLineVertical(y1, y2, x2-1, colorArgb);
        }

        /// <summary>
        /// Draws a normal filled rect
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="colorArgb"></param>
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

        /// <summary>
        /// Draws an inset rectangle. This style is used for elements like buttons and forms. Appears as if it would be sticking out.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="colorArgb"></param>
        /// <param name="insetColorArgb"></param>
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

        /// <summary>
        /// Draws an opposited inset rectangle. This style is used for elements like text fields. Appears as if it was hole in the background.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="colorArgb"></param>
        /// <param name="insetColorArgb"></param>
        public void DrawInsetOppositeRectFilled(uint x1, uint y1, uint x2, uint y2, uint colorArgb, uint insetColorArgb) {
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

            DrawRectFilled(x1, y1, x1 + 2, y2, insetColorArgb);
            DrawRectFilled(x1, y1, x2, y1 + 2, insetColorArgb);

            DrawRectFilled(x2 - 2, 0, x2, y2, 0xFFFFFFFF);
            DrawRectFilled(x1, y2 - 2, x2, y2, 0xFFFFFFFF);
        }

        /// <summary>
        /// Draws a string from a PSF font
        /// </summary>
        /// <param name="font"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="str"></param>
        /// <param name="color"></param>
        /// <param name="safe">Whether or not out of bounds checks should be performed</param>
        public void DrawStringPSF(PCScreenFont font, int x, int y, string str, uint color, bool safe = false) {
            var xOffset = x;

            foreach(var c in str) {
                DrawCharPSF(font, xOffset, y, c, color, safe, false);
                xOffset += font.Width;
            }
        }

        public void DrawStringPSFWithNewLines(PCScreenFont font, int x, int y, string str, uint color, bool safe = false, bool dbg = false) {
            var xOffset = x;
            var yOffset = y;

            foreach (var c in str) {
                if(c == '\n') {
                    xOffset = x;
                    yOffset += font.Height;
                    continue;
                }

                if (char.IsControl(c)) continue;

                DrawCharPSF(font, xOffset, yOffset, c, color, safe, dbg);
                xOffset += font.Width;
            }
        }

        /// <summary>
        /// Draws a character from a PSF font
        /// </summary>
        /// <param name="font">The font to use</param>
        /// <param name="x">The starting x position</param>
        /// <param name="y">The starting y position</param>
        /// <param name="c">The character to draw</param>
        /// <param name="color">The 32-bit ARGB color</param>
        /// <param name="safe">If this is true, a bounds check will be performed for each pixel. Slower but, well, safe.</param>
        public void DrawCharPSF(PCScreenFont font, int x, int y, char c, uint color, bool safe, bool dbg) {
            if (safe) {
                int p = font.Height * (byte)c;

                if (dbg) Kernel.Instance.Logger.Log(LogLevel.Verb, "x is " + x + " and y is " + y + " with char being (byte)" + (byte)c);

                for (int cy = 0; cy < font.Height; cy++) {
                    for (byte cx = 0; cx < font.Width; cx++) {
                        if ((font.Data[p + cy] & (1 << (cx))) != 0) {
                            var xOff = x + (font.Width - cx);
                            var yOff = y + cy;
                            var off = (yOff * SizeX) + xOff;

                            if (xOff >= SizeX || xOff < 0) continue;
                            if (yOff >= SizeY || yOff < 0) continue;
                            if (off >= Buffer.Length || off < 0) continue;
                            Buffer[off] = color;
                        }
                    }
                }
            } else { // lots of duplicated code, but well, performance is more important
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
        }

        /// <summary>
        /// Gets the absolute position of this element.
        /// </summary>
        /// <returns></returns>
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
