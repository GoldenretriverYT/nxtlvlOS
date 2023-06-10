using nxtlvlOS.Windowing.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Elements {
    public class Form : BufferedElement {
        private string title = "Untitled";
        public string Title => title;

        private bool titlebarEnabled;
        public bool TitlebarEnabled => titlebarEnabled;

        private uint backgroundColor = 0xFFDEDEDE;
        public uint BackgroundColor => backgroundColor;

        private uint insetColor = 0xFFBBBBBB;
        public uint InsetColor => insetColor;

        private TextButton closeButton;

        public Form() {
            closeButton = new TextButton() {
                RelativePosX = 0,
                RelativePosY = 6,
                SizeX = 16,
                SizeY = 16
            };

            closeButton.SetText("X");

            closeButton.Parent = this; // Todo: add proper AddChild method
            Children.Add(closeButton);
        }

        public override void Update() {
            closeButton.RelativePosX = (int)(SizeX - 22);

            base.Update();
        }

        public void SetTitle(string title) {
            this.title = title;
            this.SetDirty(true);
        }

        public void SetTitlebarEnabled(bool enabled) {
            this.titlebarEnabled = enabled;
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

        public override void Draw() {
            if (SizeY < 30) throw new Exception("Form must be at least 30 pixels in height");
            if (SizeX < 30) throw new Exception("Form must be at least 30 pixels in width");

            SetDirty(false);

            DrawInsetRectFilled(0, 0, SizeX, SizeY, backgroundColor, insetColor);

            if (titlebarEnabled) {
                DrawRectFilled(4, 4, SizeX-4, 24, 0xFF000072);
                DrawStringPSF(PCScreenFont.Default, 6, 6, title, 0xFFFFFFFF);
            }
        }
    }
}
