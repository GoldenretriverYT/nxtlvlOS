using Cosmos.Core;
using Cosmos.HAL;
using Cosmos.System;
using nxtlvlOS.Services;
using nxtlvlOS.Processing;
using nxtlvlOS.Windowing;
using nxtlvlOS.Windowing.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace nxtlvlOS.Apps
{
    public class TaskBar : App
    {
        private Form taskbarForm;
        private Container tasksContainer, infoContainer;
        private Label timeLabel, dateLabel;

        private int frames = 0;
        private int lastRTCSecond = 0;

        public override void Exit()
        {
            if (taskbarForm != null) WindowManager.RemoveForm(taskbarForm);
        }

        public override void Init(string[] args)
        {
            taskbarForm = new Form(SelfProcess);
            taskbarForm.RelativePosX = 0;
            taskbarForm.RelativePosY = 690;
            taskbarForm.SizeX = 1280;
            taskbarForm.SizeY = 30;
            taskbarForm.ShouldBeShownInTaskbar = false;
            taskbarForm.SetTitlebarEnabled(false);
            taskbarForm.SetTitle("Taskbar");

            tasksContainer = new();
            taskbarForm.AddChild(tasksContainer);

            infoContainer = new();
            taskbarForm.AddChild(infoContainer);

            timeLabel = new();
            timeLabel.SizeX = 100;
            timeLabel.SizeY = 16;
            timeLabel.SetText("00:00");
            timeLabel.SetHorizontalAlignment(HorizontalAlignment.Center);
            infoContainer.AddChild(timeLabel);

            dateLabel = new();
            dateLabel.SizeX = 100;
            dateLabel.SizeY = 16;
            dateLabel.RelativePosY = 16;
            dateLabel.SetText("2023-01-01");
            dateLabel.SetHorizontalAlignment(HorizontalAlignment.Center);
            infoContainer.AddChild(dateLabel);

            UpdateTasks();
            UpdateDateAndTime();

            WindowManager.AddForm(taskbarForm);
        }

        public override void Update()
        {
            frames++;

            if (RTC.Second % 2 == 0 && lastRTCSecond != RTC.Second)
            {
                UpdateTasks();
                frames = 0;
            }

            if (RTC.Second == 0 && lastRTCSecond != RTC.Second)
            {
                UpdateDateAndTime();
            }

            lastRTCSecond = RTC.Second;

            WindowManager.PutToFront(taskbarForm);
        }

        public void UpdateDateAndTime()
        {
            timeLabel.SetText(RTC.Hour.ToString().PadLeft(2, '0') + ":" + RTC.Minute.ToString().PadLeft(2, '0'));
            dateLabel.SetText(RTC.Century.ToString() + RTC.Year.ToString() + "-" + RTC.Month + "-" + RTC.DayOfTheMonth);
        }

        public void UpdateTasks()
        {
            foreach (var child in tasksContainer.Children.ToList())
            {
                tasksContainer.RemoveChild(child);
                //GCImplementation.Free(child); // i am not sure why we need this...
            }

            var startButton = new TextButton();
            startButton.RelativePosX = 4;
            startButton.RelativePosY = 3;
            startButton.SizeX = 24;
            startButton.SizeY = 24;
            startButton.SetText("NX");
            startButton.SetHorizontalAlignment(HorizontalAlignment.Left);
            startButton.SetVerticalAlignment(VerticalAlignment.Middle);

            startButton.Click = (_, _, _) =>
            {
                // For now, lets show a context menu as start menu
                ContextMenuService.Instance.ShowContextMenu(new() {
                    ("System Preferences", () => {
                        OpenSystemPreferences();
                    }),
                    ("Shutdown", () => {
                        Cosmos.HAL.Power.ACPIShutdown();
                    }),
                    ("(dbg) DumpFileTree", () => {
                        DumpFileTree();
                    }),
                }, 0, 720 - (int)taskbarForm.SizeY - (6 + 22 * 2)); // 6 (base) + 22 (height per item) * 2 (item count)
            };

            tasksContainer.AddChild(startButton);

            var xOffset = 30;
            var yOffset = 3;
            var formCount = 0;

            foreach (var formElement in WindowManager.Forms)
            {
                if (formElement is not Form form || !form.ShouldBeShownInTaskbar) continue;
                formCount++;

                var btn = new TextButton();
                btn.RelativePosX = xOffset;
                btn.RelativePosY = yOffset;
                btn.SizeX = 144;
                btn.SizeY = 24;
                btn.SetText(form.Title);
                btn.SetSafeDrawEnabled(true);

                btn.Click = (s, absoluteX, absoluteY) =>
                {
                    WindowManager.PutToFront(form);
                };

                tasksContainer.AddChild(btn);
                xOffset += 146;

                if (xOffset > 1050)
                {
                    xOffset = 4;
                    yOffset += 24;
                }
            }

            taskbarForm.SizeY = (uint)(6 + 24 * Math.Max(1, formCount / 7));
            taskbarForm.RelativePosY = (int)(720 - taskbarForm.SizeY);

            infoContainer.AdjustBoundingBoxAndAlignToParent(HorizontalAlignment.Right, VerticalAlignment.Middle, 10, 0);
        }

        static void DumpFileTree(string dir = @"0:\", int pad = 2) {
            foreach(var file in Directory.GetFiles(dir)) {
                Kernel.Instance.Logger.Log(LogLevel.Sill, new string(' ', pad) + "-" + file);
            }

            foreach(var directory in Directory.GetDirectories(dir)) {
                Kernel.Instance.Logger.Log(LogLevel.Sill, new string(' ', pad) + "-" + directory);
                DumpFileTree(dir + directory + @"\", pad + 2);
            }
        }

        static void OpenSystemPreferences() {
            ProcessManager.CreateProcess(new PreferenceApp(), "Preferences");
        }
    }
}
