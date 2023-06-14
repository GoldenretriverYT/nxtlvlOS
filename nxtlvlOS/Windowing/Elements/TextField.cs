using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.HAL;
using Cosmos.System;
using nxtlvlOS.Windowing.Elements.Shapes;
using nxtlvlOS.Windowing.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace nxtlvlOS.Windowing.Elements {
    public class TextField : BufferedElement {
        private uint backgroundColor = 0xFFDEDEDE;
        public uint BackgroundColor => backgroundColor;

        private uint insetColor = 0xFF808080;
        public uint InsetColor => insetColor;


        public string Text => frame.Text;
        public uint TextColor => frame.TextColor;
        public PCScreenFont Font => frame.Font;
        public int ScrollX => frame.ScrollX;
        public int ScrollY => frame.ScrollY;

        public int CursorPos = 0;

        public bool IsMouseDown { get; private set; } = false;

        private ScrollableTextFrame frame = new();
        private Rect cursor = new() {
            SizeX = 4,
            SizeY = 16
        };

        public TextField() {
            DrawMode = BufferDrawMode.RawCopy;

            cursor.BackgroundColor = 0xFF000000;
            cursor.Visible = false;

            frame.SetBackgroundColor(backgroundColor);
            UpdateFrameSizing();

            SizeChanged = () => {
                UpdateFrameSizing();
            };

            VisibilityChanged = () => {
                Kernel.Instance.Logger.Log(LogLevel.Info, "Visibility changed");
                UpdateFrameSizing(); // HACK: This is a hack! Required due to GC issues in Cosmos
            };

            AddElement(frame);
            AddElement(cursor);
        }

        public override void Draw() {
            SetDirty(false);

            if (IsMouseDown) {
                DrawRect(0, 0, SizeX, SizeY, 0xFF000000);
                DrawRectFilled(1, 1, SizeX - 2, SizeY - 2, backgroundColor);
            } else {
                DrawInsetOppositeRectFilled(0, 0, SizeX, SizeY, backgroundColor, insetColor);
            }
        }

        public override void Update() {
            if(WindowManager.FocusedElement == this && RTC.Second % 2 == 0) {
                cursor.Visible = true;

                var textPos = GetTextPosition(CursorPos);
                var relPos = GetRelativePositionFromTextPosition(textPos.x, textPos.y);

                cursor.RelativePosX = relPos.x + 3;
                cursor.RelativePosY = relPos.y + 3;
            } else {
                cursor.Visible = false;
            }

            base.Update();
        }

        private void UpdateFrameSizing() {
            frame.SizeX = SizeX - 6;
            frame.SizeY = SizeY - 6;
            frame.RelativePosX = 3;
            frame.RelativePosY = 3;
            frame.SetDirty(true);
        }

        private (int x, int y) GetTextPosition(int offset) {
            int x = 0;
            int y = 0;

            string[] lines = Text.Split(Environment.NewLine);

            for(int i = 0; i < offset; i++) {
                if (lines[y].Length <= x) {
                    x = 0;
                    y++;
                }else {
                    x++;
                }
            }

            return (x, y);
        }

        private (int x, int y) GetRelativePositionFromTextPosition(int textX, int textY) {
            int x = textX * Font.Width;
            int y = textY * Font.Height;

            return (x - ScrollX, y - ScrollY);
        }

        private (int x, int y) GetTextPositionFromRelativePosition(int relativeX, int relativeY) {
            int textX = (relativeX + ScrollX) / Font.Width;
            int textY = (relativeY + ScrollY) / Font.Height;

            return (textX, textY);
        }

        /*private int GetOffsetFromTextPosition(int textX, int textY) {
            int offset = 0;
            string[] lines = Text.Split(Environment.NewLine);

            for (int y = 0; y < textY; y++) {
                if (y >= lines.Length)
                    break;

                offset += lines[y].Length + Environment.NewLine.Length;
            }

            offset += textX;
            return offset;
        }*/

        private int GetOffsetFromTextPosition(int textX, int textY) {
            int offset = 0;
            string[] lines = Text.Split(Environment.NewLine);

            for (int y = 0; y < textY; y++) {
                if (y >= lines.Length)
                    break;

                offset += lines[y].Length + Environment.NewLine.Length;
            }

            if (textY < lines.Length)
                offset += textX;
            else if (textY == lines.Length && textX <= lines[textY - 1].Length)
                offset += textX;
            else
                offset = Text.Length;

            if (offset < 0) offset = 0;
            if (offset > Text.Length) offset = Text.Length;

            return offset;
        }

        public override void OnKey(KeyEvent ev) {
            if(char.IsControl(ev.KeyChar)) {
                switch(ev.Key) {
                    case ConsoleKeyEx.Backspace:
                        if (CursorPos > 0 && Text.Length > 0) {
                            SetText(Text.Remove(CursorPos - 1, 1));
                            CursorPos--;
                        }
                        break;
                    case ConsoleKeyEx.LeftArrow:
                        CursorPos--;
                        if (CursorPos < 0) CursorPos = 0;
                        break;
                    case ConsoleKeyEx.RightArrow:
                        CursorPos++;
                        if (CursorPos > Text.Length) CursorPos = Text.Length;
                        break;
                }
            }else {
                SetText(Text.Insert(CursorPos, ev.KeyChar.ToString()));
                CursorPos++;
            }
        }

        public void SetText(string text) {
            this.frame.SetText(text);
        }

        public void SetFont(PCScreenFont font) {
            this.frame.SetFont(font);
        }

        public void SetTextColor(uint color) {
            this.frame.SetTextColor(color);
        }

        public void SetBackgroundColor(uint color) {
            this.backgroundColor = color;
            this.frame.SetBackgroundColor(color); // The frame also gets the background color so we dont have to perform costly PixelByPixel operations
            this.SetDirty(true);
        }

        public void SetInsetColor(uint color) {
            this.insetColor = color;
            this.SetDirty(true);
        }

        public void SetScrollX(int scrollX) {
            this.frame.SetScrollX(scrollX);
        }

        public void SetScrollY(int scrollY) {
            this.frame.SetScrollY(scrollY);
        }

        public override void OnMouseDown(MouseState state) {
            base.OnMouseDown(state);

            IsMouseDown = true;
            this.SetDirty(true);
        }

        public override void OnMouseUp(MouseState state, bool mouseIsOver) {
            base.OnMouseUp(state, mouseIsOver);

            var relativeMouse = GetRelativeFromAbsolutePosition((int)MouseManager.X, (int)MouseManager.Y);
            var textPos = GetTextPositionFromRelativePosition(relativeMouse.x, relativeMouse.y);

            CursorPos = GetOffsetFromTextPosition(textPos.x, textPos.y);
            IsMouseDown = false;
            this.SetDirty(true);
        }

        public class ScrollableTextFrame : BufferedElement {
            private string text = "TextField";
            public string Text => text;

            private PCScreenFont font = PCScreenFont.Default;
            public PCScreenFont Font => font;

            private uint textColor = 0xFF000000;
            public uint TextColor => textColor;

            private uint backgroundColor = 0xFFDEDEDE;
            private uint BackgroundColor => backgroundColor;

            private int scrollX = 0;
            private int scrollY = 0;

            public int ScrollX => scrollX;
            public int ScrollY => scrollY;

            private string textCache = "";
            private bool overflowX = false, overflowY = false;


            public bool IsMouseDown { get; private set; } = false;


            public ScrollableTextFrame() {
                DrawMode = BufferDrawMode.RawCopy;
                SizeChanged = () => {
                    Kernel.Instance.Logger.Log(LogLevel.Info, "Size changed for ScrollableTextFrame with text " + text);
                };
            }

            public override void Update() {
                if(text != textCache) {
                    textCache = text;

                    (overflowX, overflowY) = GetOverflowAxes();
                }

                base.Update();
            }

            public override void Draw() {
                Kernel.Instance.Logger.Log(LogLevel.Info, "Drawing ScrollableTextFrame with text " + text + "; buffer size is " + Buffer.Length);

                SetDirty(false);
                DrawRectFilled(0, 0, SizeX, SizeY, backgroundColor); // see note in SetBackgroundColor of TextField
                DrawStringPSFWithNewLines(font, -ScrollX, -ScrollY, text, textColor, true, false);
            }

            public void SetText(string text) {
                this.text = text;
                this.SetDirty(true);
            }

            public void SetFont(PCScreenFont font) {
                this.font = font;
                this.SetDirty(true);
            }

            public void SetTextColor(uint color) {
                this.textColor = color;
                this.SetDirty(true);
            }

            public void SetScrollX(int scrollX) {
                this.scrollX = scrollX;
                this.SetDirty(true);
            }

            public void SetScrollY(int scrollY) {
                this.scrollY = scrollY;
                this.SetDirty(true);
            }

            public void SetBackgroundColor(uint backgroundColor) {
                this.backgroundColor = backgroundColor;
                this.SetDirty(true);
            }

            public override void OnMouseDown(MouseState state) {
                WindowManager.FocusedElement = Parent;
                Parent.OnMouseDown(state);
            }

            public override void OnMouseUp(MouseState state, bool mouseIsOver) {
                Parent.OnMouseUp(state, mouseIsOver);
            }

            private (bool x, bool y) GetOverflowAxes() {
                var measurement = font.MeasureStringExhaustive(text);

                return (measurement.w > SizeX, measurement.h > SizeY);
            }
        }
    }
}
