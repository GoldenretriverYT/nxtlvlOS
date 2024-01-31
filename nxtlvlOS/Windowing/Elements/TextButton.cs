using Cosmos.System;
using nxtlvlOS.Windowing.Fonts;
using nxtlvlOS.Windowing.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Elements {
    public class TextButton : BufferedElement {
        private string text = "Button Text";
        public string Text {
            get => text;
            set {
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

        private uint textColor = ColorUtils.Light100;
        public uint TextColor {
            get => textColor;
            set {
                textColor = value;
                SetDirty(true);
            }
        }

        private uint backgroundColor = ColorUtils.Primary300;
        public uint BackgroundColor {
            get => backgroundColor;
            set {
                backgroundColor = value;
                SetDirty(true);
            }
        }

        private uint pressedColor = ColorUtils.Primary100;
        public uint PressedColor {
            get => pressedColor;
            set {
                pressedColor = value;
                SetDirty(true);
            }
        }

        public HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center;
        public HorizontalAlignment HorizontalAlignment {
            get => horizontalAlignment;
            set {
                horizontalAlignment = value;
                SetDirty(true);
            }
        }

        public VerticalAlignment verticalAlignment = VerticalAlignment.Middle;
        public VerticalAlignment VerticalAlignment {
            get => verticalAlignment;
            set {
                verticalAlignment = value;
                SetDirty(true);
            }
        }

        public bool safeDrawEnabled = false;
        public bool SafeDrawEnabled {
            get => safeDrawEnabled;
            set {
                safeDrawEnabled = value;
                SetDirty(true);
            }
        }

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
