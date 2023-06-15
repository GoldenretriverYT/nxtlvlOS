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

        public HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left;
        public HorizontalAlignment HorizontalAlignment => horizontalAlignment;

        public VerticalAlignment verticalAlignment = VerticalAlignment.Top;
        public VerticalAlignment VerticalAlignment => verticalAlignment;

        public bool safeDrawEnabled = false;
        public bool SafeDrawEnabled => safeDrawEnabled;


        public Label() {
            DrawMode = BufferDrawMode.PixelByPixel;
        }

        public override void Draw() {
            Clear(0x00000000);

            if (horizontalAlignment == HorizontalAlignment.Left && verticalAlignment == VerticalAlignment.Top) {
                DrawStringPSF(font, 0, 0, text, color, safeDrawEnabled);
            } else {
                var offsets = font.AlignWithin(text, horizontalAlignment, verticalAlignment, SizeX, SizeY);
                DrawStringPSF(font, (int)offsets.x, (int)offsets.y, text, color, safeDrawEnabled);
            }
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

        public void SetHorizontalAlignment(HorizontalAlignment horizontalAlignment) {
            this.horizontalAlignment = horizontalAlignment;
            this.SetDirty(true);
        }

        public void SetVerticalAlignment(VerticalAlignment verticalAlignment) {
            this.verticalAlignment = verticalAlignment;
            this.SetDirty(true);
        }

        public void SetSafeDrawEnabled(bool safeDrawEnabled) {
            this.safeDrawEnabled = safeDrawEnabled;
            this.SetDirty(true);
        }
    }
}
