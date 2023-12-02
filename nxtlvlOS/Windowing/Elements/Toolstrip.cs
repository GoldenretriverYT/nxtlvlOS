using nxtlvlOS.Windowing.Elements.Shapes;
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

        private int rowSize = 30;
        public int RowSize => rowSize;

        private Rect line = new();

        public Toolstrip() {
            ShouldBeDrawnToScreen = false; 
            SizeY = 30;

            line.CustomId = "Line";

            AddChild(line);
        }

        public void SetLineColor(uint color) {
            lineColor = color;
            SetDirty(true);
        }

        public void SetRowSize(int size) {
            rowSize = size;
            SetDirty(true);
            DoLayout();
        }

        public override void Draw() {
        }

        public override void DoLayout() {
            int currentRow = 0;
            int currentOffsetX = 0;

            foreach(var child in Children) {
                if (child.CustomId == "Line") continue;
                
                child.SizeY = (uint)rowSize;

                if (currentOffsetX + child.SizeX > SizeX) {
                    currentRow++;
                    currentOffsetX = 0;
                }
                
                child.RelativePosX = currentOffsetX;
                child.RelativePosY = currentRow * rowSize;

                currentOffsetX += (int)child.SizeX + 2;
            }

            SizeY = (uint)((uint)(currentRow + 1) * rowSize) + 1;

            line.SizeX = SizeX;
            line.SizeY = 1;
            line.RelativePosX = 0;
            line.RelativePosY = (int)(SizeY);
            line.BackgroundColor = lineColor;
        }
    }


}
