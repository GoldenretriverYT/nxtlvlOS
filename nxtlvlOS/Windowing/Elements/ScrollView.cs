using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Elements {
    internal class ScrollView : Layout {
        public uint ContainerSizeX {
            get => innerContainer.SizeX;
            set {
                innerContainer.SizeX = value;
                DoLayout();
            }
        }

        public uint ContainerSizeY {
            get => innerContainer.SizeY;
            set {
                innerContainer.SizeY = value;
                DoLayout();
            }
        }

        private int _scrollX, _scrollY;

        public int ScrollX {
            get => _scrollX;
            set {
                _scrollX = value;
                DoLayout();
            }
        }

        public int ScrollY {
            get => _scrollY;
            set {
                _scrollY = value;
                DoLayout();
            }
        }

        private Container innerContainer = new();

        public ScrollView() {
            ShouldBeDrawnToScreen = false;
            AddChild(innerContainer);
            innerContainer.MousePassThrough = true;
        }
        
        // TODO: Add scrollbars
        public override void DoLayout() {
            innerContainer.RelativePosX = -ScrollX;
            innerContainer.RelativePosY = -ScrollY;
        }

        public override void Draw() {
        }

        public override void AddItem(BufferedElement element) {
            innerContainer.AddChild(element);
            DoLayout();
        }
    }
}
