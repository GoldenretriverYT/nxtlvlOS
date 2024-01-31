using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.HAL;
using Cosmos.System;
using nxtlvlOS.Processing;
using nxtlvlOS.Windowing.Fonts;
using nxtlvlOS.Windowing.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Elements {
    public class Form : BufferedElement {
        private int id = 0;
        private static int nextId = 0;

        private string title = "Untitled";
        public string Title {
            get => title;
            set {
                title = value;
                SetDirty(true);
            }
        }

        private bool titlebarEnabled;
        public bool TitlebarEnabled {
            get => titlebarEnabled;
            set {
                titlebarEnabled = value;
                SetDirty(true);
            }
        }

        private uint backgroundColor = ColorUtils.Primary400; 
        public uint BackgroundColor {
            get => backgroundColor;
            set {
                backgroundColor = value;
                SetDirty(true);
            }
        }

        private uint titlebarColor = ColorUtils.Primary500;
        public uint TitlebarColor {
            get => titlebarColor;
            set {
                titlebarColor = value;
                SetDirty(true);
            }
        }

        public bool ShouldBeShownInTaskbar = true;

        private TextButton closeButton;
        private bool isBeingDragged = false;
        private int dragOffsetX = 0, dragOffsetY = 0;

        private Process owner;
        public Process Owner => owner;

        public bool IsBeingClosed { get; private set; } = false;

        /// <summary>
        /// Invoked right before the form closes. Closing means to completely delete the form - not just to hide it.
        /// </summary>
        public Event BeforeClose = new();
        /// <summary>
        /// Invoked right after the form was closed.
        /// </summary>
        public Event Closed = new();

        public Form(Process owner) {
            this.owner = owner;
            this.id = nextId;
            nextId++;

            closeButton = new TextButton {
                RelativePosX = 0,
                RelativePosY = -20, // Negative due to the ChildRelativeOffsetY being 24
                SizeX = 16,
                SizeY = 16,
                PaddingY = 0,
                Text = "X"
            };

            closeButton.Click += (MouseState _, uint _, uint _) => {
                Close();
            };

            ChildRelativeOffsetX = 1; // Border

            AddChild(closeButton);

            Kernel.Instance.Logger.Log(LogLevel.Sill, $"Created form with id {id}");

            /*Cosmos.HAL.Global.PIT.RegisterTimer(new PIT.PITTimer(() => {
                Kernel.Instance.Logger.Log(LogLevel.Sill, $"alive:{id}");
            }, 1000000000, true));*/
        }

        public override void Update() {
            //Kernel.Instance.Logger.Log(LogLevel.Sill, $"alive:{id}");
            if(isBeingDragged) {
                RelativePosX = (int)MouseManager.X + dragOffsetX;
                RelativePosY = (int)MouseManager.Y + dragOffsetY;
            }

            closeButton.RelativePosX = (int)(SizeX - 22);
            closeButton.Visible = titlebarEnabled;

            var parentSize = Parent == null ? WindowManager.ScreenSize : (w: Parent.SizeX, h: Parent.SizeY);

            if (RelativePosX < 0) RelativePosX = 0;
            if (RelativePosY < 0) RelativePosY = 0;
            if (RelativePosX + SizeX > parentSize.w) RelativePosX = (int)(parentSize.w - SizeX);
            if (RelativePosY + SizeY > parentSize.h) RelativePosY = (int)(parentSize.h - SizeY);

            base.Update();
        }

        public void SetCloseButtonEnabled(bool enabled) {
            closeButton.SetEnabled(enabled);
        }

        public override void OnMouseDown(MouseState state) {
            base.OnMouseDown(state);

            if((state & MouseState.Left) == MouseState.Left && titlebarEnabled) {
                var absolutePos = GetAbsolutePosition();

                // We can be sure about the X pos since the title bar spans across the whole width
                if(MouseManager.Y > absolutePos.y && MouseManager.Y < absolutePos.y+28) {
                    isBeingDragged = true;
                    dragOffsetX = (int)absolutePos.x - (int)MouseManager.X;
                    dragOffsetY = (int)absolutePos.y - (int)MouseManager.Y;

                    WindowManager.PutToFront(this);
                }
            }
        }

        public override void OnMouseUp(MouseState state, MouseState prev, bool mouseIsOver) {
            base.OnMouseUp(state, prev, mouseIsOver);

            isBeingDragged = false;
        }

        public override void Draw() {
            SetDirty(false);
            
            if (!ShouldBeDrawnToScreen) return; // We want to support this to allow root-level overlays in the WM
            if ((titlebarEnabled && SizeY < 30) || SizeY < 10) throw new Exception("Form must be at least 30 pixels in height if title bar is enabled, or 10 if not.");
            if ((titlebarEnabled && SizeX < 30) || SizeX < 10) throw new Exception("Form must be at least 30 pixels in width if title bar is enabled, or 10 if not.");
            ChildRelativeOffsetY = (uint)(titlebarEnabled ? 24 : 1);


            DrawRectFilled(1, 1, SizeX-1, SizeY-1, backgroundColor);
            DrawRect(0, 0, SizeX, SizeY, titlebarColor);

            if (titlebarEnabled) {
                DrawRectFilled(0, 0, SizeX, 24, titlebarColor);
                DrawString(WindowManager.DefaultFont, 6, 4, title, 0xFFFFFFFF);
            }
        }

        /// <summary>
        /// Close the form - this removes it from its parent or the window manager as well.
        /// </summary>
        public unsafe void Close() {
            if(IsBeingClosed) return;

            IsBeingClosed = true;
            BeforeClose.Invoke();

            if (Parent != null) {
                Parent.RemoveChild(this);
            } else {
                WindowManager.RemoveForm(this);
            }

            Closed.Invoke();


            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@#((%@@@@@@@@@@
            // @@/,..*(&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&#*,..*/&@@@@@@@
            // @@/,,,,..,(@@@@%//*/#@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&(/,,.,*#@@@@@@
            // @@&(*,.,,*,,*((,..,,/@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&#(((%@@#/*,,.,/&@@@@@
            // @@@@(**,,....,,,,,/(&@@@@@@@@@@@&%#/*,,,,*/((((#&@@@@@@@@@(*...,(%(*,..,*(@@@@@@
            // @@@@%/*,,,,,,...,/&@@@@@@@@@%(****,,,...,,*,,,.,,*/%@@@@@@&#/,,*(/,,,,...,%@@@@@
            // @@@@@@/*,,,,....*(&@@@@@@@(***/*,,*,,,,,,,**,****,,**(@@@@@%(*,,,,.,,*,,,,%@@@@@
            // @@@@@@@/,,,..,,,*/&@@@@@#*****,,,,*/(#%%%#(/*,,,,,,,,*/#@@@%/*,,,.,,,,,,*(@@@@@@
            // @@@@@@@&/,,,...*(%@@@@@(*,,,,,,*(%&&&&&&&&&&&%/*,,,,,,,*(@@@#/*,.,,,,,*(@@@@@@@@
            // @@@@@@@@@%///**#@@@@@@(*,,,,,,(%&&&@@&&&&@@&&&&#/,,,,,,,,#@@@@&(/*//#@@@@@@@@@@@
            // @@@@@@@@@@@@@@@@@@@@@@*,,,,,*(&&&@@@@@@@@@@@&&&&%(,,,,,,,*%@@@@@@@@@@@@@@@@@@@@@
            // @@@@@@@@@@@@@@@@@@@@@&*,,,,*(&&&&@@@@@@@@@@@@&&&&%/*,,,,,*%@@@@@@@@@@@@@@@@@@@@@
            // @@@@@@@@@@@@@@@@@@@@@@*,,,*(%&&&&@@@&#((#%&@@&&&&&%/,,,,,/%@@@@@@@@@@@@@@@@@@@@@
            // @@@@@@@@@@@@@@@@@@@@@@%*,,/#&&&&&#/,......,*(%&&&&&#/,,,,%@@@@@@@@@@@@@@@@@@@@@@
            // @@@@@@@@@@@@@@@@@@@@@@@#**(%&&&#/,,,,,,,,,,,,*(%&&&%(*,*/@@@@@@@@@@@@@@@@@@@@@@@
            // @@@@@@@@@@@@@@@@@@@@@@@@#*(%&#/,,,,,,,,,,,,,,,,*(%&&(**(@@@@@@@@@@@@@@@@@@@@@@@@
            // @@@@@@@@@@@@@@@@@@@@@@@@%/,,**,,,,,,,,,,,,,,,,,,,****,*(@@@@@@@@@@@@@@@@@@@@@@@@
            // @@@@@@@@@@@@@@@@@@@@@@@@@@@&/*,,,,,,,,,,,,,,,,,,,***,,/@@@@@@@@@@@@@@@@@@@@@@@@@
            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@(/****,,***,***//(@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&%%%%%##%&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            //Heap.Free(GCImplementation.GetPointer(this)); // RISKY! Might cause memory corruption?
        }
    }
}
