﻿using Cosmos.System;
using nxtlvlOS.Processing;
using nxtlvlOS.Windowing;
using nxtlvlOS.Windowing.Elements;
using nxtlvlOS.Windowing.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Services {
    public class ContextMenuService : App {
        public static ContextMenuService Instance;
        public Form ContextMenuForm;

        public override void Exit() {
            throw new Exception("ContextMenuService should not be killed.");
        }

        public override void Init(string[] args) {
            if (Instance != null)
                throw new Exception("ContextMenuService should not be started twice.");

            Instance = this;

            ContextMenuForm = new(SelfProcess);
            ContextMenuForm.SizeX = 300;
            ContextMenuForm.Visible = false;
            ContextMenuForm.ShouldBeShownInTaskbar = false;

            WindowManager.GlobalMouseUpEvent.Subscribe((eventData) => {
                if (!ContextMenuForm.Visible) return;

                var absPos = ContextMenuForm.GetAbsolutePosition();

                if(!ShapeCollisions.RectIntersectsWithPoint(
                    absPos.y, absPos.x, absPos.y + ContextMenuForm.SizeY, absPos.x + ContextMenuForm.SizeX,
                    eventData.x, eventData.y)) {
                    ContextMenuForm.Visible = false;
                }
            });

            WindowManager.AddForm(ContextMenuForm);
        }

        public override void Update() {
        }

        public void ShowContextMenu(List<(string title, Action action)> items, int customX = -1, int customY = -1) {
            foreach (var child in ContextMenuForm.Children.ToList()) {
                ContextMenuForm.RemoveElement(child);
            }
            
            var yOffset = 0;

            foreach(var item in items) {
                var button = new TextButton();
                button.RelativePosX = 3;
                button.RelativePosY = 3 + yOffset;
                button.SizeX = 294;
                button.SizeY = 22;
                button.SetText(item.title);
                button.Click = (MouseState state, uint absX, uint absY) => {
                    item.action();
                    ContextMenuForm.Visible = false;
                };

                ContextMenuForm.AddChild(button);
                yOffset += 22;
            }

            ContextMenuForm.SizeY = (uint)(yOffset + 6);
            
            ContextMenuForm.RelativePosX = (customX == -1 ? (int)MouseManager.X : customX);
            ContextMenuForm.RelativePosY = (customY == -1 ? (int)MouseManager.Y : customY);

            if(ContextMenuForm.RelativePosY + ContextMenuForm.SizeY > 720) {
                ContextMenuForm.RelativePosY = (int)(720 - ContextMenuForm.SizeY);
            }

            if(ContextMenuForm.RelativePosX + ContextMenuForm.SizeX > 1280) {
                ContextMenuForm.RelativePosX = (int)(1280 - ContextMenuForm.SizeX);
            }

            Kernel.Instance.Logger.Log(LogLevel.Info, "Showing context menu with sizex=" + ContextMenuForm.SizeX + " sizey=" + ContextMenuForm.SizeY + " relx=" + ContextMenuForm.RelativePosX + " rely=" + ContextMenuForm.RelativePosY);
            ContextMenuForm.Visible = true;
            WindowManager.PutToFront(ContextMenuForm);
        }
    }
}
