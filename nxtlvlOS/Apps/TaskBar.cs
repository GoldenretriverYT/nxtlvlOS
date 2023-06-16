using Cosmos.Core;
using Cosmos.HAL;
using Cosmos.System;
using nxtlvlOS.Processing;
using nxtlvlOS.Windowing;
using nxtlvlOS.Windowing.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Apps {
    public class TaskBar : App {
        private Form taskbarForm;
        private Container tasksContainer, infoContainer;
        private Label timeLabel, dateLabel;

        private int frames = 0;

        public override void Exit() {
            if (taskbarForm != null) WindowManager.RemoveForm(taskbarForm);
        }

        public override void Init() {
            taskbarForm = new Form(SelfProcess);
            taskbarForm.RelativePosX = 0;
            taskbarForm.RelativePosY = 690;
            taskbarForm.SizeX = 1280;
            taskbarForm.SizeY = 30;
            taskbarForm.ShouldBeShownInTaskbar = false;
            taskbarForm.SetTitlebarEnabled(false);
            taskbarForm.SetTitle("Taskbar");

            tasksContainer = new();
            taskbarForm.AddElement(tasksContainer);

            infoContainer = new();
            taskbarForm.AddElement(infoContainer);

            timeLabel = new();
            timeLabel.SizeX = 100;
            timeLabel.SizeY = 16;
            timeLabel.SetText("00:00");
            timeLabel.SetHorizontalAlignment(HorizontalAlignment.Center);
            infoContainer.AddElement(timeLabel);

            dateLabel = new();
            dateLabel.SizeX = 100;
            dateLabel.SizeY = 16;
            dateLabel.RelativePosY = 16;
            dateLabel.SetText("2023-01-01");
            dateLabel.SetHorizontalAlignment(HorizontalAlignment.Center);
            infoContainer.AddElement(dateLabel);

            UpdateTasks();
            UpdateDateAndTime();

            WindowManager.AddForm(taskbarForm);
        }

        public override void Update() {
            frames++;

            if(RTC.Second % 2 == 0) {
                UpdateTasks();
                frames = 0;
            }

            if(RTC.Second == 0) {
                UpdateDateAndTime();
            }
            
            WindowManager.PutToFront(taskbarForm);
        }

        public void UpdateDateAndTime() {
            timeLabel.SetText(RTC.Hour + ":" + RTC.Minute);
            dateLabel.SetText(RTC.Century.ToString() + RTC.Year.ToString() + "-" + RTC.Month + "-" + RTC.DayOfTheMonth);
        }

        public void UpdateTasks() {
            foreach (var child in tasksContainer.Children.ToList()) {
                tasksContainer.RemoveElement(child);
                GCImplementation.Free(child); // i am not sure why we need this...
            }

            var startButton = new TextButton();
            startButton.RelativePosX = 4;
            startButton.RelativePosY = 3;
            startButton.SizeX = 24;
            startButton.SizeY = 24;
            startButton.SetText("NX");
            startButton.SetHorizontalAlignment(HorizontalAlignment.Left);
            startButton.SetVerticalAlignment(VerticalAlignment.Middle);
            tasksContainer.AddElement(startButton);

            var xOffset = 30;
            var yOffset = 3;
            var formCount = 0;

            foreach(var formElement in WindowManager.Forms) {
                if (formElement is not Form form || !form.ShouldBeShownInTaskbar) continue;
                formCount++;

                var btn = new TextButton();
                btn.RelativePosX = xOffset;
                btn.RelativePosY = yOffset;
                btn.SizeX = 144;
                btn.SizeY = 24;
                btn.SetText(form.Title);
                btn.SetSafeDrawEnabled(true);

                btn.Click = (MouseState s, uint absoluteX, uint absoluteY) => {
                    WindowManager.PutToFront(form);
                };

                tasksContainer.AddElement(btn);
                xOffset += 150;

                if(xOffset > 1050) {
                    xOffset = 4;
                    yOffset += 24;
                }
            }

            taskbarForm.SizeY = (uint)(6 + (24 * Math.Max(1, formCount / 7)));
            taskbarForm.RelativePosY = (int)(720 - taskbarForm.SizeY);

            infoContainer.AdjustToBoundingBox(HorizontalAlignment.Right, VerticalAlignment.Middle, 10, 0);
        }
    }
}
