using Cosmos.Core;
using Cosmos.HAL;
using Cosmos.System;
using nxtlvlOS.Services;
using nxtlvlOS.Loaders;
using nxtlvlOS.Processing;
using nxtlvlOS.Windowing;
using nxtlvlOS.Windowing.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nxtlvlOS.Utils;

namespace nxtlvlOS.Apps
{
    public class Desktop : App
    {
        private Form desktopForm;
        private Container fileContainer;

        private NXTBmp iconBmp = new(Assets.AssetManager.FileIconGeneric); // TODO: Move icon to some FileExtensionService that provides various info about an extension
        private NXTBmp directoryBmp = null; // Either retrieved from /System/Icons/directory.bmp or iconBmp as fallback
        
        private string desktopDir => UACService.UserDir + @"Desktop\";
        private int previousSecond = -1;

        public override void Exit()
        {
            if (desktopForm != null) WindowManager.RemoveForm(desktopForm);
        }

        public override void Init(string[] args)
        {
            if (File.Exists(@"0:\System\Icons\directory.bmp")) {
                var data = File.ReadAllBytes(@"0:\System\Icons\directory.bmp");
                directoryBmp = new NXTBmp(data);
            } else {
                directoryBmp = iconBmp;
            }
            
            desktopForm = new Form(SelfProcess);
            desktopForm.RelativePosX = 0;
            desktopForm.RelativePosY = 0;
            desktopForm.SizeX = 1280;
            desktopForm.SizeY = 720;
            desktopForm.ShouldBeDrawnToScreen = false; // We will use the EmptyBuffer of WindowManager for backgrounds instead, improves performance
            desktopForm.DoNotBringToFront = true;
            desktopForm.SetTitlebarEnabled(false);
            desktopForm.SetTitle("Desktop");

            desktopForm.KeyPress = (ev) =>
            {
                if (ev.Key == ConsoleKeyEx.F5)
                {
                    ReloadFiles();
                }
            };

            desktopForm.MouseUp = (state, prev, absoluteX, absoluteY) =>
            {
                Kernel.Instance.Logger.Log(LogLevel.Info, "Desktop mouse up " + (int)(prev & MouseState.Right));
                if ((prev & MouseState.Right) == MouseState.Right)
                {
                    Kernel.Instance.Logger.Log(LogLevel.Info, "Desktop right clicked");
                    ContextMenuService.Instance.ShowContextMenu(new() {
                        ("Create new file", () => {
                            CreateNewFile();
                            ReloadFiles();
                        }),
                        ("Create new directory", () => {
                            CreateNewDirectory();
                            ReloadFiles();
                        }),
                        ("Refresh (F5)", () => {
                            ReloadFiles();
                        }),
                    });
                }
            };

            fileContainer = new Container();
            desktopForm.AddChild(fileContainer);

            if (!Directory.Exists(desktopDir))
            {
                Kernel.Instance.Logger.Log(LogLevel.Info, "Creating missing desktop dir");
                Directory.CreateDirectory(desktopDir);
            }

            ReloadFiles();

            WindowManager.AddForm(desktopForm);
        }

        public override void Update()
        {
            // Listing files is super slow, lets just skip that and only do it on user request
            //if(RTC.Second % 5 == 0) {
            //    ReloadFiles();
            //}
        }

        public void CreateNewFile()
        {
            string[] alreadyExistingFiles = Directory.GetFiles(desktopDir);

            string newName = "New.txt";
            int i = 0;

            while (alreadyExistingFiles.Contains(newName))
            {
                newName = "New " + i++ + ".txt";
            }

            File.WriteAllText(desktopDir + newName, "");
        }

        public void CreateNewDirectory()
        {
            var alreadyExistingDirs = Directory.GetDirectories(desktopDir);

            string newName = "New";
            int i = 0;

            while (alreadyExistingDirs.Contains(newName))
            {
                newName = "New " + i++;
            }

            Directory.CreateDirectory(desktopDir + newName);
        }


        public void ReloadFiles()
        {
            foreach (var child in fileContainer.Children.ToList())
            {
                fileContainer.RemoveChild(child);
            }

            int offsetX = 0, offsetY = 0;

            foreach (var directory in Directory.GetDirectories(desktopDir)) {
                void DirectoryClicked(MouseState state, uint absX, uint absY) {
                    if ((state & MouseState.Left) == MouseState.Left) {
                        FileAssociationService.Instance.StartAppFromPath(desktopDir + directory, new string[] { });
                    } else if ((state & MouseState.Right) == MouseState.Right) {
                        ContextMenuService.Instance.ShowContextMenu(new() {
                            ("Open", () => {
                                // TODO: Add explorer
                            }),
                            ("Rename", () => {
                                // TODO: Add rename directory form
                            }),
                            ("Delete", () => {
                                Directory.Delete(desktopDir + directory);
                                ReloadFiles();
                            }),
                        });
                    }
                }

                ImageLabel img = new();
                img.RelativePosX = 5 + 16 + offsetX;
                img.RelativePosY = 5 + offsetY;
                img.SizeX = 64;
                img.SizeY = 64;
                img.SetTransparent(true);
                img.SetImage(directoryBmp.Data);
                img.Click = DirectoryClicked;
                fileContainer.AddChild(img);

                Label lbl = new();
                lbl.RelativePosX = 5 + offsetX;
                lbl.RelativePosY = 69 + offsetY;
                lbl.SizeX = 96;
                lbl.SizeY = 16;
                lbl.SetText(directory);
                lbl.SetHorizontalAlignment(HorizontalAlignment.Center);
                lbl.SetSafeDrawEnabled(true);
                lbl.Click = DirectoryClicked;
                fileContainer.AddChild(lbl);

                offsetY += 80;

                if (offsetY > 480) {
                    offsetY = 0;
                    offsetX += 100;
                }
            }

            foreach (var file in Directory.GetFiles(desktopDir))
            {
                void FileClicked(MouseState state, uint absX, uint absY) {
                    if ((state & MouseState.Left) == MouseState.Left) {
                        FileAssociationService.Instance.StartAppFromPath(desktopDir + file, new string[] { });
                    } else if ((state & MouseState.Right) == MouseState.Right) {
                        ContextMenuService.Instance.ShowContextMenu(new() {
                            ("Open", () => {
                                FileAssociationService.Instance.StartAppFromPath(desktopDir + file, new string[] { });
                            }),
                            ("Rename", () => {
                                RenameFile(desktopDir + file);
                            }),
                            ("Delete", () => {
                                File.Delete(desktopDir + file);
                                ReloadFiles();
                            }),
                        });
                    }
                }

                AssociationFile assoc = new() {
                    Name = "Unknown",
                    NativeTarget = "NotepadApp",
                };

                ErrorOr.Resolve(FileAssociationService.Instance.GetAssocFromPath(file), out assoc, assoc);

                NXTBmp icon = assoc.Icon ?? iconBmp;

                if (File.Exists(assoc.IconPath)) {
                    icon = new(File.ReadAllBytes(assoc.IconPath));
                }
                
                ImageLabel img = new();
                img.RelativePosX = 5 + 16 + offsetX;
                img.RelativePosY = 5 + offsetY;
                img.SizeX = 64;
                img.SizeY = 64;
                img.SetTransparent(true);
                img.SetImage(icon.Data);
                img.Click = FileClicked;
                fileContainer.AddChild(img);

                Label lbl = new();
                lbl.RelativePosX = 5 + offsetX;
                lbl.RelativePosY = 69 + offsetY;
                lbl.SizeX = 96;
                lbl.SizeY = 16;
                lbl.SetText(file);
                lbl.SetHorizontalAlignment(HorizontalAlignment.Center);
                lbl.SetSafeDrawEnabled(true);
                lbl.Click = FileClicked;
                fileContainer.AddChild(lbl);

                offsetY += 80;

                if (offsetY > 480)
                {
                    offsetY = 0;
                    offsetX += 100;
                }
            }
        }

        private void RenameFile(string originalPath) {
            Form form = new(SelfProcess);

            form.RelativePosX = 200;
            form.RelativePosY = 200;

            form.SizeX = 400;
            form.SizeY = 130;

            form.SetTitle("Rename file");
            form.SetTitlebarEnabled(true);

            Label lbl = new();
            lbl.RelativePosX = 10;
            lbl.RelativePosY = 15;
            lbl.SizeX = 390;
            lbl.SizeY = 16;
            lbl.SetText("Specify a new name for '" + Path.GetFileName(originalPath) + "':");

            TextField newNameField = new();
            newNameField.RelativePosX = 10;
            newNameField.RelativePosY = 35;
            newNameField.SizeX = 380;
            newNameField.SizeY = 24;

            newNameField.SetText(Path.GetFileName(originalPath));

            TextButton cancelBtn = new();
            cancelBtn.RelativePosX = 10;
            cancelBtn.RelativePosY = 70;
            cancelBtn.SizeX = 185;
            cancelBtn.SizeY = 24;
            cancelBtn.SetText("Cancel");
            cancelBtn.Click = (state, x, y) => {
                WindowManager.RemoveForm(form);
            };

            TextButton okBtn = new();
            okBtn.RelativePosX = 205;
            okBtn.RelativePosY = 70;
            okBtn.SizeX = 185;
            okBtn.SizeY = 24;
            okBtn.SetText("OK");
            okBtn.Click = (state, x, y) => {
                Kernel.Instance.Logger.Log(LogLevel.Verb, "Renaming file '" + originalPath + "' to '" + (Path.GetDirectoryName(originalPath) + "\\" + newNameField.Text) + "'");
                File.WriteAllBytes(Path.GetDirectoryName(originalPath) + "/" + newNameField.Text, File.ReadAllBytes(originalPath));
                File.Delete(originalPath);
                ReloadFiles();
                WindowManager.RemoveForm(form);
            };

            form.AddChild(lbl);
            form.AddChild(newNameField);
            form.AddChild(cancelBtn);
            form.AddChild(okBtn);

            WindowManager.AddForm(form);
        }
    }
}
