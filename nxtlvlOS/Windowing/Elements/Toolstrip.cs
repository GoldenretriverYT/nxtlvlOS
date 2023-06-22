using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Elements {
    /// <summary>
    /// Provides a single line of elements - theoretically supports any element, recommended is ToolstripButton however
    /// </summary>
    public class Toolstrip : Layout {
        private uint lineColor = 0xFF808080;
        public uint LineColor => lineColor;

        public Toolstrip() {
            SizeX = 0;
            ShouldBeDrawnToScreen = false;
        }

        public override void Draw() {
                   
        }

        public override void DoLayout() {

        }
    }


}
