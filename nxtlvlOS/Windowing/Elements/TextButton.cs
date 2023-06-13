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

        private PCScreenFont font = PCScreenFont.Default;
        public PCScreenFont Font => font;

        private uint textColor = 0xFF000000;
        public uint TextColor => textColor;

        private uint backgroundColor = 0xFFDEDEDE;
        public uint BackgroundColor => backgroundColor;

        private uint insetColor = 0xFF808080;
        public uint InsetColor => insetColor;

        public HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left;
        public HorizontalAlignment HorizontalAlignment => horizontalAlignment;

        public VerticalAlignment verticalAlignment = VerticalAlignment.Top;
        public VerticalAlignment VerticalAlignment => verticalAlignment;

        public bool IsMouseDown { get; private set; } = false;
        public Action<MouseState, uint, uint> Click = (MouseState state, uint absoluteX, uint absoluteY) => { };


        public TextButton() {
            DrawMode = BufferDrawMode.RawCopy;
        }

        public override void Draw() {
            SetDirty(false);

            if (IsMouseDown) {
                DrawRect(0, 0, SizeX, SizeY, 0xFF000000);
                DrawRectFilled(1, 1, SizeX-2, SizeY-2, backgroundColor);
            } else {
                DrawInsetRectFilled(0, 0, SizeX, SizeY, backgroundColor, insetColor);
            }

            if (horizontalAlignment == HorizontalAlignment.Left && verticalAlignment == VerticalAlignment.Top) {
                DrawStringPSF(font, 3, 3, text, textColor);
            }else {
                var offsets = font.AlignWithin(text, horizontalAlignment, verticalAlignment, SizeX - 6, SizeY - 6);
                DrawStringPSF(font, (int)(3 + offsets.x), (int)(3 + offsets.y), text, textColor);
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

        public override void OnMouseDown(MouseState state) {
            base.OnMouseDown(state);

            IsMouseDown = true;
            this.SetDirty(true);
        }

        public override void OnMouseUp(MouseState state, bool isMouseOver) {
            base.OnMouseUp(state, isMouseOver);

            if(isMouseOver) {
                Click(state, MouseManager.X, MouseManager.Y);
            }

            IsMouseDown = false;
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
