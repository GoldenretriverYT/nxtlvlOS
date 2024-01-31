using Cosmos.System;
using nxtlvlOS.Services;
using nxtlvlOS.Windowing.Fonts;
using nxtlvlOS.Windowing.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace nxtlvlOS.Windowing.Elements {
    /// <summary>
    /// A basic dropdown using the ContextMenuService to show its elements
    /// </summary>
    internal class BasicDropdown : BufferedElement {
        public int SelectedIndex { get; set; } = -1;
        public string SelectedElement => SelectedIndex == -1 ? null : Elements[SelectedIndex];
        public string[] Elements { get; set; } = new string[0];

        private Font font = WindowManager.DefaultFont;
        public Font Font {
            get => font;
            set {
                font = value;
                this.SetDirty(true);
            }
        }

        private uint textColor = ColorUtils.Light100;
        public uint TextColor {
            get => textColor;
            set {
                textColor = value;
                this.SetDirty(true);
            }
        }

        private uint backgroundColor = ColorUtils.Primary500;
        public uint BackgroundColor {
            get => backgroundColor;
            set {
                backgroundColor = value;
                this.SetDirty(true);
            }
        }

        private uint pressedColor = ColorUtils.Primary100;
        public uint PressedColor {
            get => pressedColor;
            set {
                pressedColor = value;
                this.SetDirty(true);
            }
        }

        public HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left;
        public HorizontalAlignment HorizontalAlignment {
            get => horizontalAlignment;
            set {
                horizontalAlignment = value;
                this.SetDirty(true);
            }
            }

        public VerticalAlignment verticalAlignment = VerticalAlignment.Middle;
        public VerticalAlignment VerticalAlignment {
            get => verticalAlignment;
            set {
                verticalAlignment = value;
                this.SetDirty(true);
            }
        }

        public bool safeDrawEnabled = false;
        public bool SafeDrawEnabled => safeDrawEnabled;

        public bool IsMouseDown { get; private set; } = false;

        public override void Draw() {
            if (SizeX < 64) {
                throw new Exception("BasicDropdown needs to be at least 64 pixels wide");
            }

            SetDirty(false);

            if (SelectedIndex >= Elements.Length) {
                SelectedIndex = -1;
            }

            string shownText = SelectedIndex == -1 ? "Select" : Elements[SelectedIndex];

            if (IsMouseDown && enabled) {
                DrawRectFilled(0, 0, SizeX, SizeY, pressedColor);
            } else {
                DrawRectFilled(0, 0, SizeX, SizeY, backgroundColor);
            }

            if (horizontalAlignment == HorizontalAlignment.Left && verticalAlignment == VerticalAlignment.Top) {
                DrawString(font, 3, 3, shownText, textColor, safeDrawEnabled);
            } else {
                var offsets = font.AlignWithin(shownText, horizontalAlignment, verticalAlignment, SizeX - 22, SizeY - 6);
                DrawString(font, (int)(3 + offsets.x), (int)(3 + offsets.y), shownText, textColor, safeDrawEnabled);
            }

            DrawString(font, (int)(SizeX - 19), 3, "v", textColor, safeDrawEnabled);
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

            if ((prev & MouseState.Left) != 0 && isMouseOver) {
                List<(string title, Action action)> actions = new();

                for (int i = 0; i < Elements.Length; i++) {
                    int index = i;
                    actions.Add((Elements[i], () => {
                        SelectedIndex = index;
                        this.SetDirty(true);
                    }
                    ));
                }

                var absolutePos = GetAbsolutePosition();
                
                ContextMenuService.Instance.ShowContextMenu(actions, (int)absolutePos.x, (int)(absolutePos.y + SizeY), (int)SizeX);
            }

            IsMouseDown = false;
            this.SetDirty(true);
        }

        public void SetSafeDrawEnabled(bool safeDrawEnabled) {
            this.safeDrawEnabled = safeDrawEnabled;
            this.SetDirty(true);
        }

        public void AddElement(string element) {
            List<string> elements = Elements.ToList();
            elements.Add(element);
            Elements = elements.ToArray();
        }

        public void RemoveElement(string element) {
            List<string> elements = Elements.ToList();
            elements.Remove(element);
            Elements = elements.ToArray();
        }

        public void SelectElement(string element) {
            SelectedIndex = Elements.ToList().IndexOf(element);
            this.SetDirty(true);
        }
    }
}
