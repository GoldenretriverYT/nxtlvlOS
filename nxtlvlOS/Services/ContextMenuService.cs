using Cosmos.System;
using nxtlvlOS.Processing;
using nxtlvlOS.Windowing;
using nxtlvlOS.Windowing.Elements;
using nxtlvlOS.Windowing.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nxtlvlOS.Services {
    public class ContextMenuService : App {
        public static ContextMenuService Instance;
        public Form ContextMenuForm;
        private bool makeVisible = false;

        public override void Exit() {
            throw new Exception("ContextMenuService should not be killed.");
        }

        public override void Init(string[] args) {
            if (Instance != null)
                throw new Exception("ContextMenuService should not be started twice.");

            Instance = this;

            ContextMenuForm = new(SelfProcess) {
                SizeX = 300,
                Visible = false,
                ShouldBeShownInTaskbar = false
            };

            WindowManager.GlobalMouseUpEvent += (eventData) => {
                if (ContextMenuForm == null) {
                    Kernel.Instance.Logger.Log(LogLevel.Warn, "ContextMenuForm is null; should not be null.");
                    return;
                }

                if (!ContextMenuForm.Visible) return;

                var absPos = ContextMenuForm.GetAbsolutePosition();

                if (!ShapeCollisions.RectIntersectsWithPoint(
                    absPos.y, absPos.x, absPos.y + ContextMenuForm.SizeY, absPos.x + ContextMenuForm.SizeX,
                    eventData.x, eventData.y)) {
                    ContextMenuForm.Visible = false;
                }
            };

            WindowManager.AddForm(ContextMenuForm);
        }

        public override void Update() {
            if (makeVisible) {                            // This will delay the context menu from showing
                                                          // until the next frame, but this is required as newly
                                                          // create elements (in this case buttons) can not
                                                          // be shown within the same frame as they are created.
                ContextMenuForm.Visible = true;
                makeVisible = false;
            }
            
            if(ContextMenuForm.Visible) {
                WindowManager.PutToFront(ContextMenuForm);
            }
        }

        public void ShowContextMenu(List<(string title, Action action)> items, int customX = -1, int customY = -1, int customWidth = 300) {
            foreach (var child in ContextMenuForm.Children.ToList()) {
                ContextMenuForm.RemoveChild(child);
                child.IsDeleted = true;
            }

            ContextMenuForm.SizeX = (uint)customWidth;

            var yOffset = 0;

            foreach(var item in items) {
                var button = new TextButton {
                    RelativePosX = 3,
                    RelativePosY = 3 + yOffset,
                    SizeX = ContextMenuForm.SizeX - 6,
                    SizeY = 22,
                    Text = (item.title),
                    HorizontalAlignment = (HorizontalAlignment.Left)
                };

                button.Click += (MouseState state, uint absX, uint absY) => {
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
            makeVisible = true;
            WindowManager.PutToFront(ContextMenuForm);
        }
    }
}
