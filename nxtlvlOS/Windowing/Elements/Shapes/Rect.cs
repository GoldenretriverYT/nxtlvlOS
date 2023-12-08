using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Elements.Shapes {
    /// <summary>
    /// Draws a plain rectangle on the screen
    /// </summary>
    public class Rect : BufferedElement {
        public uint backgroundColor = 0xFF000000;
        public uint BackgroundColor {
            get {
                return backgroundColor;
            }
            
            set {
                backgroundColor = value;
                this.SetDirty(true);
            }
        }

        public override void Draw() {
            SetDirty(false);
            DrawRectFilled(0, 0, SizeX, SizeY, backgroundColor);
        }
    }
}
