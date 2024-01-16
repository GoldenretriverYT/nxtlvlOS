using Cosmos.System;
using nxtlvlOS.Windowing.Elements;
using nxtlvlOS.Windowing.Fonts;
using nxtlvlOS.Windowing.Utils;
using System.Collections.Generic;

namespace nxtlvlOS.Windowing {
    public abstract class BufferedElement {
        public BufferedElement Parent;
        public List<BufferedElement> Children = new();

        public int RelativePosX, RelativePosY;
        public uint SizeX = 100, SizeY = 100;
        public bool Visible = true;
        public uint[] Buffer = new uint[0];
        public bool DoNotBringToFront = false;
        public string CustomId = "NoId";

        /// <summary>
        /// If this is true, this elements becomes uneligible for all mouse events (down, up, hover start, hover end).
        /// </summary>
        public bool MousePassThrough = false;

        /// <summary>
        /// This boolean is to mark that the element will never be used again. At that point, it should already be removed
        /// from the parent.
        /// 
        /// This is required as the window manager flattens all elements into a new list before updating and
        /// reuses that list on draw, which can cause unparented elements to have an absolute position of 0,0.
        /// 
        /// This does not mark the element for the GC or whatever, it only prevents the Copy Buffer step
        /// of the window manager from copying the buffer of this element.
        /// </summary>
        public bool IsDeleted = false;

        public uint ChildRelativeOffsetX = 0, ChildRelativeOffsetY = 0;

        /// <summary>
        /// This does not affect child elements, unlike <see cref="Visible"/> does - use this for layout elements
        /// Also prevents a buffer from being created, which <see cref="Visible"/> does not.
        /// </summary>
        public bool ShouldBeDrawnToScreen = true;

        private uint _bufSizeX = 0, _bufSizeY = 0;
        protected bool dirty = false;

        public BufferDrawMode DrawMode = BufferDrawMode.RawCopy;

        public bool enabled = true;
        /// <summary>
        /// Elements can choose to use the property as they please. Built-in interactive elements use this
        /// to decide whether or not the element should be interactive. WARNING! CHANGING FORCES A REDRAW!
        /// 
        /// Note: As not all elements need this, a SetEnabled method is up to the element itself to implement.
        /// (THIS MIGHT CHANGE IN THE FUTURE)
        /// </summary>
        public bool Enabled => enabled;

        /// <summary>
        /// If the buffer was updated since the last time this was set to false.
        /// This will be used as follows:
        ///   1) Element.Update
        ///   2) WindowManager/ParentElement checks if this is true
        ///   3) if it is, copies buffer to its own buffer
        ///      or if its not, continues with the next element
        /// </summary>
        public bool BufferWasUpdated = true;

        public Event PreDrawAndChildUpdate = new();
        public Event PostDrawAndChildUpdate = new();

        public Event SizeChanged = new();
        public Event VisibilityChanged = new();

        public Event<MouseState, uint, uint> Click = new();
        
        public Event<MouseState, uint, uint> MouseDown = new();
        public Event<MouseState, MouseState, uint, uint> MouseUp = new();
        public Event<int> MouseScroll = new();

        public Event<KeyEvent> KeyPress = new();

        public bool VisibleIncludingParents => Visible && (Parent == null ? true : Parent.VisibleIncludingParents);
        public bool PreviousVisibility = false;
        public bool ScrollPassThrough = true; // We dont react to scroll by default

        public virtual void Update() {
            PreDrawAndChildUpdate.Invoke();

            if (ShouldBeDrawnToScreen && (_bufSizeX != SizeX || _bufSizeY != SizeY)) {
                _bufSizeX = SizeX;
                _bufSizeY = SizeY;
                var finalSize = (SizeY * SizeX);

                if (finalSize < 0 || finalSize > 0x7FFFFFFF) {
                    Kernel.Instance.Logger.Log(LogLevel.Crit, $"Buffer size of {GetType().Name} ({CustomId}) is negative or too high! ({SizeY * SizeX}) SizeY: {SizeY} SizeX: {SizeX}");
                    return;
                }

                Buffer = new uint[finalSize];
                SetDirty(true);
                SizeChanged.Invoke();
            }

            if(PreviousVisibility != VisibleIncludingParents) {
                PreviousVisibility = VisibleIncludingParents;
                VisibilityChanged.Invoke();
            }


            if(dirty) {
                //Kernel.Instance.Logger.Log(LogLevel.Sill, $"Drawing {GetType().Name} ({CustomId}) because it is dirty");
                Draw();
                BufferWasUpdated = true;
            }


            foreach(var child in Children) {
                child.Update();
            }

            PostDrawAndChildUpdate.Invoke();
        }

        public virtual void OnMouseDown(MouseState state) {
            this.BringToFront();

            MouseDown.Invoke(state, MouseManager.X, MouseManager.Y);
        }

        public virtual void OnMouseUp(MouseState state, MouseState prev, bool mouseIsOver) {
            MouseUp.Invoke(state, prev, MouseManager.X, MouseManager.Y);

            if (mouseIsOver && enabled) {
                Click.Invoke(prev, MouseManager.X, MouseManager.Y); // we need to provide prev so the mouse button can get fetched from it
            }
        }

        public virtual void OnMouseScroll(int delta) {
            MouseScroll.Invoke(delta);
        }

        public virtual void OnHoverStart() {
            // TODO: Implement this
        }

        public virtual void OnHoverEnd() {
            // TODO: Implement this
        }

        public virtual void OnKey(KeyEvent ev) {
            KeyPress.Invoke(ev);
        }

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

        public void SetDirty(bool isDirty) {
            this.dirty = isDirty;
        }

        public void AddChild(BufferedElement el) {
            el.Parent = this;
            Children.Add(el);
        }

        public bool RemoveChild(BufferedElement el) {
            if(Children.Contains(el)) {
                Children.Remove(el);
                el.Parent = null;
                return true;
            }

            return false;
        }

        public abstract void Draw();

        public (int x, int y) GetRelativeFromAbsolutePosition(int absoluteX, int absoluteY) {
            var thisAbsolute = GetAbsolutePosition();
            absoluteX -= (int)thisAbsolute.x;
            absoluteY -= (int)thisAbsolute.y;

            return (absoluteX, absoluteY);
        }

        public (int xMin, int xMax, int yMin, int yMax) GetParentBounds() {
            if (Parent == null) {
                return (0, (int)WindowManager.ScreenSize.w, 0, (int)WindowManager.ScreenSize.h);
            }

            var parentAbs = Parent.GetAbsolutePosition();
            var parentBounds = Parent.GetParentBounds();

            // If any size is 0, we use the screen size because its probably 0 as the element has no size
            var xMin = (int)parentAbs.x;
            var xMax = (int)parentAbs.x + (int)(Parent.SizeX == 0 ? WindowManager.ScreenSize.w : Parent.SizeX);
            var yMin = (int)parentAbs.y;
            var yMax = (int)parentAbs.y + (int)(Parent.SizeY == 0 ? WindowManager.ScreenSize.h : Parent.SizeY);

            if (xMin < parentBounds.xMin) xMin = parentBounds.xMin;
            if (xMax > parentBounds.xMax) xMax = parentBounds.xMax;
            if (yMin < parentBounds.yMin) yMin = parentBounds.yMin;
            if (yMax > parentBounds.yMax) yMax = parentBounds.yMax;

            return (xMin, xMax, yMin, yMax);
        }

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
            DrawLineHorizontal(x1, x2, y2-1, colorArgb);
            DrawLineVertical(y1, y2, x2-1, colorArgb);
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

        public void DrawString(Font font, int x, int y, string str, uint color, bool safe = false) {
            var xOffset = x;

            foreach (var c in str) {
                DrawChar(font, xOffset, y, c, color, safe, false);
                xOffset += font.Width;
            }
        }

        public void DrawStringWithNewLines(Font font, int x, int y, string str, uint color, bool safe = false, bool dbg = false) {
            var xOffset = x;
            var yOffset = y;

            foreach (var c in str) {
                if(c == '\n') {
                    xOffset = x;
                    yOffset += font.Height;
                    continue;
                }

                if (char.IsControl(c)) continue;

                DrawChar(font, xOffset, yOffset, c, color, safe, dbg);
                xOffset += font.Width;
            }
        }

        /// <summary>
        /// Draws a character from a font
        /// </summary>
        /// <param name="font">The font to use</param>
        /// <param name="x">The starting x position</param>
        /// <param name="y">The starting y position</param>
        /// <param name="c">The character to draw</param>
        /// <param name="color">The 32-bit ARGB color</param>
        /// <param name="safe">If this is true, a bounds check will be performed for each pixel. Slower but, well, safe.</param>
        public void DrawChar(Font font, int x, int y, char c, uint color, bool safe, bool dbg) {
            font.DrawChar(this, x, y, c, color, safe, dbg);
        }

        public (uint x, uint y) GetAbsolutePosition() {
            uint x = (uint)RelativePosX;
            uint y = (uint)RelativePosY;

            BufferedElement par = Parent;

            while(par != null) {
                x += (uint)par.RelativePosX + par.ChildRelativeOffsetX;
                y += (uint)par.RelativePosY + par.ChildRelativeOffsetY;

                par = par.Parent;
            }

            return (x, y);
        }
    }
}
