using nxtlvlOS.Windowing.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Elements {
    /// <summary>
    /// A container to hold many child elements; used for structurizing - has no effect on layout apart from relative positioning
    /// However, a container has utility functions like GetBoundingBox, AdjustToBoundingBox (with center support)
    /// </summary>
    public class Container : BufferedElement {
        public Container() {
            SizeX = 0;
            SizeY = 0;
            ShouldBeDrawnToScreen = false;
        }

        public void EnableDebug() {
            ShouldBeDrawnToScreen = true;
        }

        public override void Draw() {
            if (ShouldBeDrawnToScreen && SizeX != 0 && SizeY != 0) {
                DrawRect(0, 0, SizeX, SizeY, 0xFFFF0000);
            }
        }

        public (int left, int top, int right, int bottom) GetRelativeBoundingBox() {
            var left = 999999999;
            var top = 999999999;
            var right = -999999999;
            var bottom = -999999999;

            foreach(var el in Children) {
                if(el.RelativePosX < left) {
                    left = el.RelativePosX;
                }

                if(el.RelativePosY < top) {
                    top = el.RelativePosY;
                }

                if(el.RelativePosX + (int)el.SizeX > right) {
                    right = (el.RelativePosX + (int)el.SizeX);
                }

                if(el.RelativePosY + (int)el.SizeY > bottom) {
                    bottom = (el.RelativePosY + (int)el.SizeY);
                }
            }

            return (left, top, right, bottom);
        }

        public void AdjustBoundingBoxAndAlignToParent(HorizontalAlignment horizontal = HorizontalAlignment.Left, VerticalAlignment vertical = VerticalAlignment.Top, int paddingX = 0, int paddingY = 0) {
            var boundingBox = GetRelativeBoundingBox();
            var parentSize = (
                x: Parent.SizeX - Parent.ChildRelativeOffsetX - (paddingX * 2),
                y: Parent.SizeY - Parent.ChildRelativeOffsetY - (paddingY * 2));
            SizeX = (uint)(boundingBox.right - boundingBox.left);
            SizeY = (uint)(boundingBox.bottom - boundingBox.top);

            if (horizontal == HorizontalAlignment.Left) {
                RelativePosX = paddingX;
            }else if(horizontal == HorizontalAlignment.Center) {
                RelativePosX = (int)(parentSize.x - SizeX) / 2;
            } else if (horizontal == HorizontalAlignment.Right) {
                RelativePosX = (int)(parentSize.x - SizeX);
            }

            if (vertical == VerticalAlignment.Top) {
                RelativePosY = paddingY;
            } else if (vertical == VerticalAlignment.Middle) {
                RelativePosY = (int)(parentSize.y - SizeY) / 2;
            }else if(vertical == VerticalAlignment.Bottom) {
                RelativePosY = (int)(parentSize.y - SizeY*2);
            }
        }
    }
}
