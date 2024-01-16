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

        private Font font = WindowManager.DefaultFont;
        public Font Font => font;

        private uint textColor = 0xFFFFFFFF;
        public uint TextColor => textColor;

        private uint backgroundColor = 0xFF666688;
        public uint BackgroundColor => backgroundColor;

        private uint pressedColor = 0xFF8888AA;
        public uint PressedColor => pressedColor;

        public HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center;
        public HorizontalAlignment HorizontalAlignment => horizontalAlignment;

        public VerticalAlignment verticalAlignment = VerticalAlignment.Middle;
        public VerticalAlignment VerticalAlignment => verticalAlignment;

        public bool safeDrawEnabled = false;
        public bool SafeDrawEnabled => safeDrawEnabled;

        private int paddingX = 3;
        public int PaddingX {
            get => paddingX;
            set {
                paddingX = value;
                SetDirty(true);
            }
        }

        private int paddingY = 3;
        public int PaddingY {
            get => paddingY;
            set {
                paddingY = value;
                SetDirty(true);
            }
        }

        public bool IsMouseDown { get; private set; } = false;
        public bool IsMouseHovering { get; private set; } = false;


        public TextButton() {
            DrawMode = BufferDrawMode.RawCopy;
        }

        public override void Draw() {
            SetDirty(false);
            
            if (IsMouseDown && enabled) {
                DrawRectFilled(0, 0, SizeX, SizeY, pressedColor);
            } else {
                if (!IsMouseHovering) {
                    DrawRectFilled(0, 0, SizeX, SizeY, backgroundColor);
                } else {
                    DrawRect(0, 0, SizeX, SizeY, 0xFF000000);
                    DrawRectFilled(1, 1, SizeX - 1, SizeY - 1, backgroundColor);
                }
            }

            if (horizontalAlignment == HorizontalAlignment.Left && verticalAlignment == VerticalAlignment.Top) {
                DrawString(font, paddingX, paddingY, text, textColor, safeDrawEnabled);
            }else {
                var offsets = font.AlignWithin(text, horizontalAlignment, verticalAlignment, SizeX - 6, SizeY - 6);
                DrawString(font, (int)(paddingX + offsets.x), (int)(paddingY + offsets.y), text, textColor, safeDrawEnabled);
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

        public void SetPressedColor(uint color) {
            this.pressedColor = color;
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

        public override void OnHoverStart() {
            IsMouseHovering = true;
            this.SetDirty(true);
        }

        public override void OnHoverEnd() {
            IsMouseHovering = false;
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
