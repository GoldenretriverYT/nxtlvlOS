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

        public override void Update() {
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



        public override void Draw() {
            if (SizeY < 20) throw new Exception("Form must be at least 20 pixels in height");

            SetDirty(false);
            if(titlebarEnabled) DrawRectFilled(0, 0, SizeX, 20, 0xFF878787);
            DrawRectFilled(0, (titlebarEnabled ? 20u : 0u), SizeX, SizeY, backgroundColor);
        }
    }
}
