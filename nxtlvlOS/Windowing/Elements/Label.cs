using nxtlvlOS.Windowing.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Elements {
    public class Label : BufferedElement {
        private string text = "Label Text";
        public string Text { get => text; set {
                text = value;
                SetDirty(true);
            }
        }

        private Font font = WindowManager.DefaultFont;
        public Font Font {
            get => font;
            set {
                font = value;
                SetDirty(true);
            }
        }

        private uint color = 0xFFFFFFFF;
        public uint Color {
            get => color;
            set {
                color = value;
                SetDirty(true);
            }
        }

        private HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left;
        public HorizontalAlignment HorizontalAlignment {
            get => horizontalAlignment;
            set {
                horizontalAlignment = value;
                SetDirty(true);
            }
        }

        private VerticalAlignment verticalAlignment = VerticalAlignment.Top;
        public VerticalAlignment VerticalAlignment {
            get => verticalAlignment;
            set {
                verticalAlignment = value;
                SetDirty(true);
            }
        }

        private bool safeDrawEnabled = false;
        public bool SafeDrawEnabled {
            get => safeDrawEnabled;
            set {
                safeDrawEnabled = value;
                SetDirty(true);
            }
        }

        private bool newlinesEnabled = false;
        public bool NewlinesEnabled {
            get => newlinesEnabled;
            set {
                newlinesEnabled = value;
                SetDirty(true);
            }
        }


        public Label() {
            DrawMode = BufferDrawMode.PixelByPixel;
        }

        public override void Draw() {
            SetDirty(false);
            Clear(0x00000000);

            if (horizontalAlignment == HorizontalAlignment.Left && verticalAlignment == VerticalAlignment.Top) {
                if (!newlinesEnabled)
                    DrawString(font, 0, 0, text, color, safeDrawEnabled, false);
                else
                    DrawStringWithNewLines(font, 0, 0, text, color, safeDrawEnabled, false, false);
            } else {
                var offsets = font.AlignWithin(text, horizontalAlignment, verticalAlignment, SizeX, SizeY);

                if (!newlinesEnabled)
                    DrawString(font, (int)offsets.x, (int)offsets.y, text, color, safeDrawEnabled, false);
                else
                    DrawStringWithNewLines(font, (int)offsets.x, (int)offsets.y, text, color, safeDrawEnabled, false, false);
            }
        }
    }
}
