using Cosmos.System;
using nxtlvlOS.Windowing.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Elements {
    public class TextField : BufferedElement {
        private string text = "TextField";
        public string Text => text;

        private PCScreenFont font = PCScreenFont.Default;
        public PCScreenFont Font => font;

        private uint textColor = 0xFF000000;
        public uint TextColor => textColor;

        private uint backgroundColor = 0xFFDEDEDE;
        public uint BackgroundColor => backgroundColor;

        private uint insetColor = 0xFF808080;
        public uint InsetColor => insetColor;

        private int scrollX = 0;
        private int scrollY = 0;

        public int ScrollX => scrollX;
        public int ScrollY => scrollY;


        public bool IsMouseDown { get; private set; } = false;


        public TextField() {
            DrawMode = BufferDrawMode.RawCopy;
        }

        public override void Draw() {
            SetDirty(false);

            if (IsMouseDown) {
                DrawRect(0, 0, SizeX, SizeY, 0xFF000000);
                DrawRectFilled(1, 1, SizeX - 2, SizeY - 2, backgroundColor);
            } else {
                DrawInsetOppositeRectFilled(0, 0, SizeX, SizeY, backgroundColor, insetColor);
            }

            DrawStringPSFWithNewLines(font, 3, 3, text, textColor, true);
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

        public void SetBackgroundColor(uint color) {
            this.backgroundColor = color;
            this.SetDirty(true);
        }

        public void SetInsetColor(uint color) {
            this.insetColor = color;
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

        public override void OnMouseDown(MouseState state) {
            base.OnMouseDown(state);

            IsMouseDown = true;
            this.SetDirty(true);
        }

        public override void OnMouseUp(MouseState state) {
            base.OnMouseUp(state);

            IsMouseDown = false;
            this.SetDirty(true);
        }
    }
}
