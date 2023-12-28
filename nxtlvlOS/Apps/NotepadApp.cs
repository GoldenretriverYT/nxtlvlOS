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
                form.SetTitle("Notepad - " + (value ?? "Untitled"));
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
            toolstrip.RelativePosY = 0;

            toolstrip.SizeX = 600;
            toolstrip.SetRowSize(24);
            
            var fileMenu = new ToolstripButton();
            fileMenu.SetText("File");
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

            var editMenu = new ToolstripButton();
            editMenu.SetText("Edit");
            editMenu.Click += (state, x, y) => {
                ContextMenuService.Instance.ShowContextMenu(new() {
                    ("Cut", () => {}),
                    ("Copy", () => {}),
                    ("Paste", () => {}),
                }, (int)(editMenu.GetAbsolutePosition().x), (int)(editMenu.GetAbsolutePosition().y + editMenu.SizeY), 200);
            };

            var helpMenu = new ToolstripButton();
            helpMenu.SetText("Help");
            helpMenu.Click += (state, x, y) => {
                ContextMenuService.Instance.ShowContextMenu(new() {
                    ("About Notepad", () => { About(); })
                }, (int)(helpMenu.GetAbsolutePosition().x), (int)(helpMenu.GetAbsolutePosition().y + helpMenu.SizeY), 200);
            };

            toolstrip.AddItem(fileMenu);
            toolstrip.AddItem(editMenu);
            toolstrip.AddItem(helpMenu);

            // TextField
            textField = new();
            textField.RelativePosX = 200;
            textField.RelativePosY = 26; // toolstrip is included within the size
                                         // note: the toolstrip is actually 25px to show a seperator
            textField.SizeX = 600 - 4;
            textField.SizeY = 400 - 52; // 52 = toolstrip (25px) + titlebar (24px) + 4px padding

            textField.SetBackgroundColor(0xFFFFFFFF);
            textField.SetTextColor(0xFF000000);

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
            textField.SetText(res);
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
        
        private void SaveDirectly() {
            if(CurrentFilePath == null) {
                return;
            }

            File.WriteAllText(CurrentFilePath, textField.Text);
        }

        private void About() {
            Form aboutForm = new(SelfProcess);
            aboutForm.RelativePosX = form.RelativePosX + 50;
            aboutForm.RelativePosY = form.RelativePosY + 50;

            aboutForm.SizeX = 200;
            aboutForm.SizeY = 200;

            aboutForm.SetTitle("About - Notepad");
            aboutForm.SetTitlebarEnabled(true);

            Container container = new();

            Label appName = new();

            appName.SetText("Notepad");
            appName.RelativePosX = 0;
            appName.RelativePosY = 0;
            appName.SetHorizontalAlignment(HorizontalAlignment.Center);
            appName.SizeY = 20;
            appName.SizeX = 150;

            Label authorName = new();

            authorName.SetText("by GoldenretriverYT");
            authorName.RelativePosX = 0;
            authorName.RelativePosY = 40;
            authorName.SetHorizontalAlignment(HorizontalAlignment.Center);
            authorName.SizeY = 20;
            authorName.SizeX = 160;

            Label authorNameLine2 = new();

            authorNameLine2.SetText("& Contributors");
            authorNameLine2.RelativePosX = 0;
            authorNameLine2.RelativePosY = 60;
            authorNameLine2.SetHorizontalAlignment(HorizontalAlignment.Center);
            authorNameLine2.SizeY = 20;
            authorNameLine2.SizeX = 150;

            container.AddChild(appName);
            container.AddChild(authorName);
            container.AddChild(authorNameLine2);

            aboutForm.AddChild(container);
            container.AdjustBoundingBoxAndAlignToParent(HorizontalAlignment.Center, VerticalAlignment.Middle);

            WindowManager.AddForm(aboutForm);
        }
    }
}
