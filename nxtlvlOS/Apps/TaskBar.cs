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
        private int lastFormsCount = 0;

        public override void Exit()
        {
            if (taskbarForm != null) WindowManager.RemoveForm(taskbarForm);
        }

        public override void Init(string[] args)
        {
            taskbarForm = new Form(SelfProcess) {
                RelativePosX = 0,
                RelativePosY = 690,
                SizeX = 1280,
                SizeY = 30,
                ShouldBeShownInTaskbar = false,
                TitlebarEnabled = (false),
                Title = ("Taskbar")
            };

            tasksContainer = new();
            taskbarForm.AddChild(tasksContainer);

            infoContainer = new();
            taskbarForm.AddChild(infoContainer);

            timeLabel = new() {
                SizeX = 100,
                SizeY = 16,
                RelativePosY = 0,
                Text = "00:00",
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            infoContainer.AddChild(timeLabel);

            dateLabel = new() {
                SizeX = 100,
                SizeY = 16,
                RelativePosY = 16,
                Text = "2023-01-01",
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            infoContainer.AddChild(dateLabel);

            UpdateTasks();
            UpdateDateAndTime();

            WindowManager.AddForm(taskbarForm);
        }

        public override void Update()
        {
            frames++;

            if ((RTC.Second % 2 == 0 && lastRTCSecond != RTC.Second) ||
                (WindowManager.Forms.Count != lastFormsCount)) {
                UpdateTasks();
                frames = 0;
            }

            if (RTC.Second == 0 && lastRTCSecond != RTC.Second)
            {
                UpdateDateAndTime();
            }

            lastRTCSecond = RTC.Second;
            lastFormsCount = WindowManager.Forms.Count;

            WindowManager.PutToFront(taskbarForm);
        }

        public void UpdateDateAndTime()
        {
            timeLabel.Text = RTC.Hour.ToString().PadLeft(2, '0') + ":" + RTC.Minute.ToString().PadLeft(2, '0');
            dateLabel.Text = RTC.Century.ToString() + RTC.Year.ToString() + "-" + RTC.Month + "-" + RTC.DayOfTheMonth;
        }

        public void UpdateTasks()
        {
            foreach (var child in tasksContainer.Children.ToList())
            {
                tasksContainer.RemoveChild(child);
                //GCImplementation.Free(child); // i am not sure why we need this...
            }

            var startButton = new TextButton {
                RelativePosX = 4,
                RelativePosY = 4,
                SizeX = 24,
                SizeY = 24,
                Text = ("NX"),
                HorizontalAlignment = (HorizontalAlignment.Left),
                VerticalAlignment = (VerticalAlignment.Middle)
            };

            startButton.Click += (_, _, _) =>
            {
                // For now, lets show a context menu as start menu
                // TODO: Add a real start menu
                ContextMenuService.Instance.ShowContextMenu(new() {
                    ("System Preferences", () => {
                        OpenSystemPreferences();
                    }),
                    ("File Explorer", () => {
                        OpenFileExplorer();
                    }),
                    ("Task Manager", () => {
                        OpenTaskManager();
                    }),
                    ("Shutdown", () => {
                        Cosmos.HAL.Power.ACPIShutdown();
                    }),
                    ("(dbg) DumpFileTree", () => {
                        DumpFileTree();
                    }),
                    ("(dbg) DiskTest", () => {
                        DiskTest();
                    }),
                    ("(dbg) UITest", () => {
                        UITest();
                    }),
                }, 0, 720 - (int)taskbarForm.SizeY - (6 + 22 * 5)); // 6 (base) + 22 (height per item) * 5 (item count)
            };

            tasksContainer.AddChild(startButton);

            var xOffset = 30;
            var yOffset = 4;
            var formCount = 0;

            foreach (var formElement in WindowManager.Forms)
            {
                if (formElement is not Form form || !form.ShouldBeShownInTaskbar) continue;
                formCount++;

                var btn = new TextButton {
                    RelativePosX = xOffset,
                    RelativePosY = yOffset,
                    SizeX = 144,
                    SizeY = 24,
                    Text = (form.Title)
                };
                btn.SetSafeDrawEnabled(true);

                btn.Click += (s, absoluteX, absoluteY) =>
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

            taskbarForm.SizeY = (uint)(8 + 24 * Math.Max(1, formCount / 7));
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

        static void OpenFileExplorer() {
            ProcessManager.CreateProcess(new FileExplorer(), "File Explorer");
        }

        static void OpenTaskManager() {
            ProcessManager.CreateProcess(new TaskManager(), "Task Manager");
        }

        static void DiskTest() {
            int start = RTC.Hour * 3600 + RTC.Minute * 60 + RTC.Second;
            Kernel.Instance.Logger.Log(LogLevel.Info, "Writing 8mb of data...");

            byte[] data = new byte[1024 * 1024 * 8];
            File.WriteAllBytes(@"0:\test.bin", data);

            Kernel.Instance.Logger.Log(LogLevel.Info, "Reading 8mb of data...");
            File.ReadAllBytes(@"0:\test.bin");

            int end = RTC.Hour * 3600 + RTC.Minute * 60 + RTC.Second;
            Kernel.Instance.Logger.Log(LogLevel.Info, "Done in " + (end - start) + " seconds");
        }

        void UITest() {
            Form form = new(SelfProcess) {
                SizeX = 1000,
                SizeY = 600,
                RelativePosX = 100,
                RelativePosY = 40
            };

            var button = new TextButton {
                RelativePosX = 10,
                RelativePosY = 10,
                SizeX = 100,
                SizeY = 30,
                Text = ("Click me!")
            };

            button.Click += (_, _, _) => {
                button.Text = ("Clicked!");
            };

            form.AddChild(button);

            var scrollView = new ScrollView {
                RelativePosX = 10,
                RelativePosY = 50,
                SizeX = 300,
                SizeY = 300,

                ContainerSizeX = 1000,
                ContainerSizeY = 1000
            };

            form.AddChild(scrollView);

            var label = new Label() {
                RelativePosX = 0,
                RelativePosY = 0,
                SizeX = 200,
                SizeY = 16,
                Text = "Hello world!",
            };

            scrollView.AddItem(label);

            var label2 = new Label() {
                RelativePosX = 280,
                RelativePosY = 600,
                SizeX = 200,
                SizeY = 16,
                Text = "Hello world!",
            };

            scrollView.AddItem(label2);

            var textField = new TextField {
                RelativePosX = 10,
                RelativePosY = 20,
                SizeX = 200,
                SizeY = 200,
                Text = ("Hello world!")
            };
            scrollView.AddItem(textField);

            // There currently are no scrollbars built into scrollview, add some "scroll buttons" to test scrolling
            var scrollUpButton = new TextButton {
                RelativePosX = 410,
                RelativePosY = 50,
                SizeX = 50,
                SizeY = 50,
                Text = ("Up")
            };

            scrollUpButton.Click += (_, _, _) => {
                scrollView.ScrollY += 10;
            };

            form.AddChild(scrollUpButton);

            var scrollDownButton = new TextButton {
                RelativePosX = 410,
                RelativePosY = 100,
                SizeX = 50,
                SizeY = 50,
                Text = ("Down")
            };

            scrollDownButton.Click += (_, _, _) => {
                scrollView.ScrollY -= 10;
            };

            form.AddChild(scrollDownButton);

            var scrollLeftButton = new TextButton {
                RelativePosX = 360,
                RelativePosY = 75,
                SizeX = 50,
                SizeY = 50,
                Text = ("Left")
            };

            scrollLeftButton.Click += (_, _, _) => {
                scrollView.ScrollX += 10;
            };

            form.AddChild(scrollLeftButton);

            var scrollRightButton = new TextButton {
                RelativePosX = 460,
                RelativePosY = 75,
                SizeX = 50,
                SizeY = 50,
                Text = ("Right")
            };

            scrollRightButton.Click += (_, _, _) => {
                scrollView.ScrollX -= 10;
            };

            form.AddChild(scrollRightButton);

            WindowManager.AddForm(form);
        }
    }
}
