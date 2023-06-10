using nxtlvlOS.Windowing.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Elements {
    public class Label : BufferedElement {
        private string text = "Label Text";
        public string Text => text;

        private PCScreenFont font = PCScreenFont.Default;
        public PCScreenFont Font => font;

        private uint color = 0xFF000000;
        public uint Color => color;

        public Label() {
            DrawMode = BufferDrawMode.PixelByPixel;
        }

        public override void Draw() {
            Clear(0x00000000);
            DrawStringPSF(font, 0, 0, text, color);
        }

        public void SetText(string text) {
            this.text = text;
            this.SetDirty(true);
        }

        public void SetFont(PCScreenFont font) {
            this.font = font;
            this.SetDirty(true);
        }

        public void SetColor(uint color) {
            this.color = color;
            this.SetDirty(true);
        }
    }
}
