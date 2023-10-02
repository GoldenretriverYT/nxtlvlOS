using nxtlvlOS.Processing;
using nxtlvlOS.RAMFS;
using nxtlvlOS.Services;
using nxtlvlOS.Windowing;
using nxtlvlOS.Windowing.Elements;
using System;
using System.Collections.Generic;
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
                form.SetTitle("Notepad - " + value ?? "Untitled");
            }
        }
        private string _currentFilePath = null;

        public override void Exit() {
            if (form != null) form.Close();
        }

        public override void Init(string[] args) {
            form = new(SelfProcess);
            form.RelativePosX = 100;
            form.RelativePosY = 100;

            form.SizeX = 600;
            form.SizeY = 400;

            form.SetTitle("Notepad");
            form.SetTitlebarEnabled(true);

            // Toolstrip
            toolstrip = new();
            toolstrip.RelativePosX = 0;
            toolstrip.RelativePosY = 24;

            toolstrip.SizeX = 600;
            toolstrip.SetRowSize(24);
            
            var fileMenu = new ToolstripButton();
            fileMenu.SetText("File");
            fileMenu.Click = (state, x, y) => {
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

            var editMenu = new ToolstripButton();
            editMenu.SetText("Edit");
            editMenu.Click = (state, x, y) => {
                ContextMenuService.Instance.ShowContextMenu(new() {
                    ("Cut", () => {}),
                    ("Copy", () => {}),
                    ("Paste", () => {}),
                }, (int)(editMenu.GetAbsolutePosition().x), (int)(editMenu.GetAbsolutePosition().y + editMenu.SizeY), 200);
            };

            var helpMenu = new ToolstripButton();
            helpMenu.SetText("Help");
            helpMenu.Click = (state, x, y) => {
                ContextMenuService.Instance.ShowContextMenu(new() {
                    ("About Notepad", () => {})
                }, (int)(helpMenu.GetAbsolutePosition().x), (int)(helpMenu.GetAbsolutePosition().y + helpMenu.SizeY), 200);
            };

            toolstrip.AddItem(fileMenu);
            toolstrip.AddItem(editMenu);
            toolstrip.AddItem(helpMenu);

            // TextField
            textField = new();
            textField.RelativePosX = 0;
            textField.RelativePosY = 49; // the titlebar + toolstrip is included within the size
                                         // note: the toolstrip is actually 25px to show a seperator
            textField.SizeX = 600;
            textField.SizeY = 400 - 49;
            
            textField.SetBackgroundColor(0xFFFFFFFF);
            textField.SetInsetColor(textField.BackgroundColor); // basically disable the inset effect

            form.AddChild(toolstrip);
            form.AddChild(textField);

            if (args.Length > 0) {
                CurrentFilePath = args[0];
                LoadFromCurrentFile();
            }

            WindowManager.AddForm(form);
        }

        private void LoadFromCurrentFile() {
            var res = Kernel.FS.ReadAllText(CurrentFilePath);

            if(res.IsError) {
                logger.Log(LogLevel.Fail, "Failed to load file: " + res.Error);
                // TODO: When we have a system to show message boxes, tell them the error.
                CurrentFilePath = null;
                return;
            }

            textField.SetText(res.Data);
        }

        public override void Update() {

        }

        private void New() {
            CurrentFilePath = null;
            textField.SetText("");
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
        private void SaveDirectly() { }
    }
}
