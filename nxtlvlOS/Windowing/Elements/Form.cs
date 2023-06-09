﻿using Cosmos.System;
using nxtlvlOS.Processing;
using nxtlvlOS.Windowing.Fonts;
using nxtlvlOS.Windowing.Utils;
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

        private uint insetColor = 0xFF808080;
        public uint InsetColor => insetColor;

        public bool ShouldBeShownInTaskbar = true;

        private TextButton closeButton;
        private bool isBeingDragged = false;
        private int dragOffsetX = 0, dragOffsetY = 0;

        private Process owner;
        public Process Owner => owner;

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
            closeButton = new TextButton() {
                RelativePosX = 0,
                RelativePosY = 6,
                SizeX = 16,
                SizeY = 16
            };

            closeButton.SetText("X");

            closeButton.Click = (MouseState _, uint _, uint _) => {
                Close();
            };

            AddChild(closeButton);
        }

        public override void Update() {
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

        public void SetInsetColor(uint color) {
            this.insetColor = color;
            this.SetDirty(true);
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
            if (!ShouldBeDrawnToScreen) return; // We want to support this to allow root-level overlays in the WM
            if ((titlebarEnabled && SizeY < 30) || SizeY < 10) throw new Exception("Form must be at least 30 pixels in height if title bar is enabled, or 10 if not.");
            if ((titlebarEnabled && SizeX < 30) || SizeX < 10) throw new Exception("Form must be at least 30 pixels in width if title bar is enabled, or 10 if not.");

            SetDirty(false);

            DrawInsetRectFilled(0, 0, SizeX, SizeY, backgroundColor, insetColor);

            if (titlebarEnabled) {
                DrawRectFilled(4, 4, SizeX-4, 24, 0xFF000072);
                DrawStringPSF(PCScreenFont.Default, 6, 6, title, 0xFFFFFFFF);
            }
        }

        /// <summary>
        /// Close the form - this removes it from its parent or the window manager as well.
        /// </summary>
        public void Close() {
            BeforeClose.Invoke();

            if (Parent != null) {
                Parent.RemoveElement(this);
            } else {
                WindowManager.RemoveForm(this);
            }

            Closed.Invoke();
        }
    }
}
