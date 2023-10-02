using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Elements {
    /// <summary>
    /// Docks the child elements to the left, right, top or bottom
    /// </summary>
    public class DockingLayout : Layout {
        public DockMode DockMode { get; set; } = DockMode.All;

        public override void DoLayout() {
            var left = 0;
            var top = 0;
            var right = 0;
            var bottom = 0;

            foreach (var el in Children) {
                if (el.RelativePosX < left) {
                    left = el.RelativePosX;
                }

                if (el.RelativePosY < top) {
                    top = el.RelativePosY;
                }

                if (el.RelativePosX + (int)el.SizeX > right) {
                    right = (el.RelativePosX + (int)el.SizeX);
                }

                if (el.RelativePosY + (int)el.SizeY > bottom) {
                    bottom = (el.RelativePosY + (int)el.SizeY);
                }
            }

            foreach (var el in Children) {
                if (DockMode.HasFlag(DockMode.Left)) {
                    el.RelativePosX = left;
                }

                if (DockMode.HasFlag(DockMode.Top)) {
                    el.RelativePosY = top;
                }

                if (DockMode.HasFlag(DockMode.Right)) {
                    el.RelativePosX = right - (int)el.SizeX;
                }

                if (DockMode.HasFlag(DockMode.Bottom)) {
                    el.RelativePosY = bottom - (int)el.SizeY;
                }
            }
        }

        public override void Draw() {
            SetDirty(false);
            // nothing to draw
        }
    }

    [Flags]
    public enum DockMode {
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8,

        All = Left | Right | Top | Bottom
    }
}
