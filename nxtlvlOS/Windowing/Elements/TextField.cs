using Cosmos.System;
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

        public bool IsMouseDown { get; private set; } = false;
        private ScrollableTextFrame frame = new(); 

        public TextField() {
            DrawMode = BufferDrawMode.RawCopy;

            frame.SetBackgroundColor(backgroundColor);
            UpdateFrameSizing();

            AddElement(frame);
        }

        public override void Draw() {
            SetDirty(false);
            UpdateFrameSizing();

            if (IsMouseDown) {
                DrawRect(0, 0, SizeX, SizeY, 0xFF000000);
                DrawRectFilled(1, 1, SizeX - 2, SizeY - 2, backgroundColor);
            } else {
                DrawInsetOppositeRectFilled(0, 0, SizeX, SizeY, backgroundColor, insetColor);
            }
        }

        private void UpdateFrameSizing() {
            frame.SizeX = SizeX - 6;
            frame.SizeY = SizeY - 6;
            frame.RelativePosX = 3;
            frame.RelativePosY = 3;
            frame.SetDirty(true);
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

            IsMouseDown = false;
            this.SetDirty(true);
        }

        private class ScrollableTextFrame : BufferedElement {
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


            public bool IsMouseDown { get; private set; } = false;


            public ScrollableTextFrame() {
                DrawMode = BufferDrawMode.RawCopy;
            }

            public override void Draw() {
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
                Parent.OnMouseDown(state);
            }

            public override void OnMouseUp(MouseState state, bool mouseIsOver) {
                Parent.OnMouseUp(state, mouseIsOver);
            }
        }
    }
}
