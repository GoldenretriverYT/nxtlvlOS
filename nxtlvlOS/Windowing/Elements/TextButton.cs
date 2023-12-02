using Cosmos.System;
using nxtlvlOS.Windowing.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Elements {
    public class TextButton : BufferedElement {
        private string text = "Button Text";
        public string Text => text;

        private PCScreenFont font = WindowManager.DefaultFont;
        public PCScreenFont Font => font;

        private uint textColor = 0xFF000000;
        public uint TextColor => textColor;

        private uint backgroundColor = 0xFFDEDEDE;
        public uint BackgroundColor => backgroundColor;

        private uint insetColor = 0xFF808080;
        public uint InsetColor => insetColor;

        public HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center;
        public HorizontalAlignment HorizontalAlignment => horizontalAlignment;

        public VerticalAlignment verticalAlignment = VerticalAlignment.Middle;
        public VerticalAlignment VerticalAlignment => verticalAlignment;

        public bool safeDrawEnabled = false;
        public bool SafeDrawEnabled => safeDrawEnabled;

        public bool IsMouseDown { get; private set; } = false;


        public TextButton() {
            DrawMode = BufferDrawMode.RawCopy;
        }

        public override void Draw() {
            SetDirty(false);

            if (IsMouseDown && enabled) {
                DrawRect(0, 0, SizeX, SizeY, 0xFF000000);
                DrawRectFilled(1, 1, SizeX-2, SizeY-2, backgroundColor);
            } else {
                if (enabled) {
                    DrawInsetRectFilled(0, 0, SizeX, SizeY, backgroundColor, insetColor);
                } else {
                    // todo: make a better style for disabled buttons, it looks horrible rn
                    DrawInsetOppositeRectFilled(0, 0, SizeX, SizeY, backgroundColor, insetColor);
                }
            }

            if (horizontalAlignment == HorizontalAlignment.Left && verticalAlignment == VerticalAlignment.Top) {
                DrawStringPSF(font, 3, 3, text, textColor, safeDrawEnabled);
            }else {
                var offsets = font.AlignWithin(text, horizontalAlignment, verticalAlignment, SizeX - 6, SizeY - 6);
                DrawStringPSF(font, (int)(3 + offsets.x), (int)(3 + offsets.y), text, textColor, safeDrawEnabled);
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

        public void SetHorizontalAlignment(HorizontalAlignment horizontalAlignment) {
            this.horizontalAlignment = horizontalAlignment;
            this.SetDirty(true);
        }

        public void SetVerticalAlignment(VerticalAlignment verticalAlignment) {
            this.verticalAlignment = verticalAlignment;
            this.SetDirty(true);
        }

        public void SetEnabled(bool enabled) {
            this.enabled = enabled;
            this.SetDirty(true);
        }

        public override void OnMouseDown(MouseState state) {
            base.OnMouseDown(state);

            IsMouseDown = true;
            this.SetDirty(true);
        }

        public override void OnMouseUp(MouseState state, MouseState prev, bool isMouseOver) {
            base.OnMouseUp(state, prev, isMouseOver);

            IsMouseDown = false;
            this.SetDirty(true);
        }

        public void SetSafeDrawEnabled(bool safeDrawEnabled) {
            this.safeDrawEnabled = safeDrawEnabled;
            this.SetDirty(true);
        }
    }

    public enum HorizontalAlignment {
        Left,
        Center,
        Right
    }

    public enum VerticalAlignment {
        Top,
        Middle,
        Bottom
    }
}
