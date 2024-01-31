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
        public uint LineColor {
            get => lineColor;
            set {
                lineColor = value;
                SetDirty(true);
            }
        }

        private int rowSize = 30;
        public int RowSize {
            get => rowSize;
            set {
                rowSize = value;
                SetDirty(true);
                DoLayout();
            }
        }

        private Rect line = new();

        public Toolstrip() {
            ShouldBeDrawnToScreen = false; 
            SizeY = 30;

            line.CustomId = "Line";

            AddChild(line);
        }

        public override void Draw() {
            SetDirty(false);
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
