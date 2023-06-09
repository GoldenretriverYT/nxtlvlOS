using nxtlvlOS.Windowing.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Elements {
    public class TextButton : BufferedElement {
        private string text = "Button Text";
        public string Text => text;

        private PCScreenFont font = PCScreenFont.Default;
        public PCScreenFont Font => font;

        private uint textColor = 0xFF000000;
        public uint TextColor => textColor;

        private uint backgroundColor = 0xFFDEDEDE;
        public uint BackgroundColor => backgroundColor;

        private uint insetColor = 0xFFBBBBBB;
        public uint InsetColor => insetColor;

        public TextButton() {
            DrawMode = BufferDrawMode.RawCopy;
        }

        public override void Draw() {
            DrawInsetRectFilled(0, 0, SizeX, SizeY, backgroundColor, insetColor);
            DrawStringPSF(font, 3, 0, text, textColor);
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
    }
}
