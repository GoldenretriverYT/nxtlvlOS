using Cosmos.Core;
using nxtlvlOS.Processing;
using nxtlvlOS.Services;
using nxtlvlOS.Windowing;
using nxtlvlOS.Windowing.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Apps
{
    public class NotepadApp : App {
        private NXTLogger logger = NXTLogger.Create("Notepad", true);

        private Form form;
        private TextField textField;
        private Toolstrip toolstrip;

        public string CurrentFilePath {
            get => _currentFilePath;
            set {
                _currentFilePath = value;
                form.Title = ("Notepad - " + (value ?? "Untitled"));
            }
        }
        private string _currentFilePath = null;

        public override void Exit() {
            if (form != null && !form.IsBeingClosed) form.Close();
        }

        public override unsafe void Init(string[] args) {
            form = new(SelfProcess) {
                RelativePosX = 100,
                RelativePosY = 100,

                SizeX = 600,
                SizeY = 400,

                Title = ("Notepad"),
                TitlebarEnabled = (true)
            };

            Kernel.Instance.Logger.Log(LogLevel.Info, "Form located at 0x" + new IntPtr(GCImplementation.GetPointer(form)).ToString("X8"));

            form.Closed += () => {
                ProcessManager.KillProcess(SelfProcess);
            };

            // Toolstrip
            toolstrip = new() {
                RelativePosX = 0,
                RelativePosY = 0,

                SizeX = 600,
                RowSize = (24)
            };

            var fileMenu = new ToolstripButton {
                Text = "File"
            };
            fileMenu.Click += (state, x, y) => {
                ContextMenuService.Instance.ShowContextMenu(new() {
                    ("New", () => { New(); }),
                    ("Open", () => { Open(); }),
                    ("Save", () => {
                        Save();
                    }),
                    ("Save As...", () => {
                        SaveAs();
                    }),
                }, (int)(fileMenu.GetAbsolutePosition().x), (int)(fileMenu.GetAbsolutePosition().y + fileMenu.SizeY), 200);
            };

            var editMenu = new ToolstripButton {
                Text = "Edit"
            };
            editMenu.Click += (state, x, y) => {
                ContextMenuService.Instance.ShowContextMenu(new() {
                    ("Cut", () => {}),
                    ("Copy", () => {}),
                    ("Paste", () => {}),
                }, (int)(editMenu.GetAbsolutePosition().x), (int)(editMenu.GetAbsolutePosition().y + editMenu.SizeY), 200);
            };

            var helpMenu = new ToolstripButton {
                Text = "Help"
            };
            helpMenu.Click += (state, x, y) => {
                ContextMenuService.Instance.ShowContextMenu(new() {
                    ("About Notepad", () => { About(); })
                }, (int)(helpMenu.GetAbsolutePosition().x), (int)(helpMenu.GetAbsolutePosition().y + helpMenu.SizeY), 200);
            };

            toolstrip.AddItem(fileMenu);
            toolstrip.AddItem(editMenu);
            toolstrip.AddItem(helpMenu);

            // TextField
            textField = new() {
                RelativePosX = 2,
                RelativePosY = 26, // toolstrip is included within the size
                                   // note: the toolstrip is actually 25px to show a seperator
                SizeX = 600 - 4,
                SizeY = 400 - 52, // 52 = toolstrip (25px) + titlebar (24px) + 4px padding

                BackgroundColor = (0xFFFFFFFF),
                TextColor = (0xFF000000)
            };

            form.AddChild(toolstrip);
            form.AddChild(textField);

            if (args.Length > 0) {
                CurrentFilePath = args[0];
                LoadFromCurrentFile();
            }

            WindowManager.AddForm(form);
        }

        private void LoadFromCurrentFile() {
            var res = File.ReadAllText(CurrentFilePath);
            textField.Text = (res);
        }

        public override void Update() {

        }

        private void New() {
            CurrentFilePath = null;
            textField.Text = ("");
        }

        private void Open() {

        }

        private void Save() {
            if (CurrentFilePath == null)
                SaveAs();
            else
                SaveDirectly();
        }
        
        private void SaveAs() { }
        
        private void SaveDirectly() {
            if(CurrentFilePath == null) {
                return;
            }

            File.WriteAllText(CurrentFilePath, textField.Text);
        }

        private void About() {
            Form aboutForm = new(SelfProcess) {
                RelativePosX = form.RelativePosX + 50,
                RelativePosY = form.RelativePosY + 50,

                SizeX = 200,
                SizeY = 200,

                Title = ("About - Notepad"),
                TitlebarEnabled = (true)
            };

            Container container = new();

            Label appName = new() {
                RelativePosX = 0,
                RelativePosY = 0,
                SizeX = 150,
                SizeY = 20,
                Text = "Notepad",
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            Label authorName = new() {
                RelativePosX = 0,
                RelativePosY = 40,
                SizeX = 160,
                SizeY = 20,
                Text = "by GoldenretriverYT",
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            Label authorNameLine2 = new() {
                RelativePosX = 0,
                RelativePosY = 60,
                SizeX = 150,
                SizeY = 20,
                Text = "& Contributors",
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            container.AddChild(appName);
            container.AddChild(authorName);
            container.AddChild(authorNameLine2);

            aboutForm.AddChild(container);
            container.AdjustBoundingBoxAndAlignToParent(HorizontalAlignment.Center, VerticalAlignment.Middle);

            WindowManager.AddForm(aboutForm);
        }
    }
}
