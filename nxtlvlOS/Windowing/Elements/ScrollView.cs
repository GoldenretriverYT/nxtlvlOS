using Cosmos.System;
using nxtlvlOS.Windowing.Elements.Shapes;
using nxtlvlOS.Windowing.Utils;
using System;

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

        private bool _allowHorizontalScrollBar = true, _allowVerticalScrollBar = true;

        public bool AllowHorizontalScrollBar {
            get => _allowHorizontalScrollBar;
            set {
                _allowHorizontalScrollBar = value;
                DoLayout();
            }
        }

        public bool AllowVerticalScrollBar {
            get => _allowVerticalScrollBar;
            set {
                _allowVerticalScrollBar = value;
                DoLayout();
            }
        }

        public bool HasHorizontalScrollBar => HasHorizontalOverflow && AllowHorizontalScrollBar;
        public bool HasVerticalScrollBar => HasVerticalOverflow && AllowVerticalScrollBar;

        public bool HasHorizontalOverflow => innerContainer.SizeX > SizeX;
        public bool HasVerticalOverflow => innerContainer.SizeY > SizeY;

        private Container innerContainer = new();
        private Container viewportContainer = new();
        private ScrollBar verticalScrollBar = new();
        private ScrollBar horizontalScrollBar = new();

        public ScrollView() {
            ShouldBeDrawnToScreen = false;
            ScrollPassThrough = false;

            MouseScroll += (int delta) => {
                if (HasVerticalScrollBar) {
                    ScrollY -= -delta * 10;
                    if (ScrollY < 0) ScrollY = 0;
                    if (ScrollY > innerContainer.SizeY - viewportContainer.SizeY) ScrollY = (int)(innerContainer.SizeY - viewportContainer.SizeY);
                    DoLayout();
                }else if (HasHorizontalScrollBar) { // Only do that if there is no vertical scrollbar
                    ScrollX -= -delta * 10;
                    if (ScrollX < 0) ScrollX = 0;
                    if (ScrollX > innerContainer.SizeX - viewportContainer.SizeX) ScrollX = (int)(innerContainer.SizeX - viewportContainer.SizeX);
                    DoLayout();
                }
            };

            viewportContainer.AddChild(innerContainer);
            AddChild(viewportContainer);
            AddChild(verticalScrollBar);
            AddChild(horizontalScrollBar);

            verticalScrollBar.Orentation = Orentation.Vertical;
            horizontalScrollBar.Orentation = Orentation.Horizontal;
            verticalScrollBar.Visible = false;
            horizontalScrollBar.Visible = false;

            verticalScrollBar.OnValueChanged += () => {
                ScrollY = verticalScrollBar.Value;
            };

            horizontalScrollBar.OnValueChanged += () => {
                ScrollX = horizontalScrollBar.Value;
            };

            DoLayout();
            SizeChanged += () => {
                DoLayout();
            };
            
            innerContainer.MousePassThrough = true;
        }
        
        // TODO: Add scrollbars
        public override void DoLayout() {
            innerContainer.RelativePosX = -ScrollX;
            innerContainer.RelativePosY = -ScrollY;

            viewportContainer.SizeX = HasVerticalScrollBar ? SizeX - 16 : SizeX;
            viewportContainer.SizeY = HasHorizontalScrollBar ? SizeY - 16 : SizeY;

            verticalScrollBar.RelativePosX = (int)(SizeX - 16);
            verticalScrollBar.RelativePosY = 0;
            verticalScrollBar.SizeX = 16;
            verticalScrollBar.SizeY = (uint)(SizeY - (HasHorizontalScrollBar ? 16 : 0));
            verticalScrollBar.Visible = HasVerticalScrollBar;
            verticalScrollBar.MaxValue = (int)innerContainer.SizeY - (int)viewportContainer.SizeY;
            verticalScrollBar.Value = ScrollY;

            horizontalScrollBar.RelativePosX = 0;
            horizontalScrollBar.RelativePosY = (int)(SizeY - 16);
            horizontalScrollBar.SizeX = (uint)(SizeX - (HasVerticalScrollBar ? 16 : 0));
            horizontalScrollBar.SizeY = 16;
            horizontalScrollBar.Visible = HasHorizontalScrollBar;
            horizontalScrollBar.MaxValue = (int)innerContainer.SizeX - (int)viewportContainer.SizeX;
            horizontalScrollBar.Value = ScrollX;
        }

        public override void Draw() {
        }

        public override void AddItem(BufferedElement element) {
            innerContainer.AddChild(element);
            DoLayout();
        }
    }

    class ScrollBar : Layout {
        private int maxValue = 100, value = 0;
        private Orentation orentation = Orentation.Horizontal;

        public int MaxValue {
            get => maxValue;
            set {
                maxValue = value;
                DoLayout();
            }
        }

        public int Value {
            get => value;
            set {
                this.value = value;
                DoLayout();
            }
        }

        public Orentation Orentation {
            get => orentation;
            set {
                orentation = value;
                DoLayout();
            }
        }

        private TextButton upButton = new();
        private TextButton downButton = new();
        private Rect bar = new();
        private Rect dragBar = new();

        private bool holdingDragger = false;
        private uint diffX = 0, diffY = 0;

        public Event OnValueChanged = new();

        public ScrollBar() {
            ShouldBeDrawnToScreen = false;
            SizeChanged += () => { DoLayout(); };

            upButton.SetText("^");
            downButton.SetText("v");

            bar.BackgroundColor = ColorUtils.Primary500;
            bar.DoNotBringToFront = true;
            dragBar.BackgroundColor = ColorUtils.Primary100;

            dragBar.MouseDown += StartDragging;
            dragBar.MouseUp += StopDragging;

            DoLayout();
            AddChild(upButton);
            AddChild(downButton);
            AddChild(bar);
            AddChild(dragBar);
        }

        private void StartDragging(MouseState state, uint absX, uint absY) {
            holdingDragger = true;
            diffX = (uint)Math.Abs(dragBar.GetAbsolutePosition().x - absX);
            diffY = (uint)Math.Abs(dragBar.GetAbsolutePosition().y - absY);
        }

        private void StopDragging(MouseState prevState, MouseState state, uint absX, uint absY) {
            holdingDragger = false;
        }

        public override void Update() {
            base.Update();

            if (holdingDragger) {
                var absoluteBarPos = bar.GetAbsolutePosition();

                if (orentation == Orentation.Vertical) {
                    var y = (int)(MouseManager.Y - absoluteBarPos.y - diffY);
                    if (y < 16) y = 16;
                    if (y > SizeY - 32 - dragBar.SizeY) y = (int)(SizeY - 32 - dragBar.SizeY);
                    dragBar.RelativePosY = y;
                    Value = (int)((y - 16) / (float)(SizeY - 32 - dragBar.SizeY) * MaxValue);
                    OnValueChanged.Invoke();
                } else {
                    var x = (int)(MouseManager.X - absoluteBarPos.x - diffX);
                    if (x < 16) x = 16;
                    if (x > SizeX - 32 - dragBar.SizeX) x = (int)(SizeX - 32 - dragBar.SizeX);
                    dragBar.RelativePosX = x;
                    Value = (int)((x - 16) / (float)(SizeX - 32 - dragBar.SizeX) * MaxValue);
                    OnValueChanged.Invoke();
                }
            }
        }

        public override void Draw() {
            
        }

        public override void DoLayout() {
            if (SizeX == 0 || SizeY == 0) return;
            
            upButton.RelativePosX = 0;
            upButton.RelativePosY = 0;
            upButton.SizeX = SizeX;
            upButton.SizeY = 16;

            downButton.RelativePosX = 0;
            downButton.RelativePosY = (int)(SizeY - 16);
            downButton.SizeX = SizeX;
            downButton.SizeY = 16;

            if (orentation == Orentation.Vertical) {
                bar.RelativePosX = 0;
                bar.RelativePosY = 16;
                bar.SizeX = SizeX;
                bar.SizeY = SizeY - 32;
                bar.CustomId = "ScrollBarVertBar";

                dragBar.RelativePosX = 0;
                dragBar.RelativePosY = 16 + (int)((SizeY - 32) * (Value / (float)MaxValue));
                dragBar.SizeX = SizeX;
                dragBar.SizeY = 32;
                dragBar.CustomId = "ScrollBarVertHandle";
            }else {
                bar.RelativePosX = 16;
                bar.RelativePosY = 0;
                bar.SizeX = SizeX - 32;
                bar.SizeY = SizeY;
                bar.CustomId = "ScrollBarHorizBar";

                dragBar.RelativePosX = 16 + (int)((SizeX - 32) * (Value / (float)MaxValue));
                dragBar.RelativePosY = 0;
                dragBar.SizeX = 32;
                dragBar.SizeY = SizeY;
                dragBar.CustomId = "ScrollBarHorizHandle";
            }
        }

        /// <summary>
        /// Gets the size of the drag bar.
        /// Maximum is the actual remaining size, minimum is 16.
        /// The size should be that, if there is a higher MaxValue, the dragbar is smaller.
        /// </summary>
        /// <returns></returns>
        private uint GetDragBarSizeVert() {
            uint size = (uint)((SizeY - 32) / (MaxValue / 100f));
            if (size > SizeY - 32) size = SizeY - 32;
            if (size < 16) size = 16;
            return size;
        }

        private uint GetDragBarSizeHoriz() {
            uint size = (uint)((SizeX - 32) / (MaxValue / 100f));
            if (size > SizeX - 32) size = SizeX - 32;
            if (size < 16) size = 16;
            return size;
        }
    }
}
