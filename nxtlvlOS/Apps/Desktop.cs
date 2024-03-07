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
    // TODO: Remove file functionality from desktop, let our desktop be a app launcher ONLY!
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

            desktopForm = new Form(SelfProcess) {
                RelativePosX = 0,
                RelativePosY = 0,
                SizeX = 1280,
                SizeY = 720,
                ShouldBeDrawnToScreen = false, // We will use the EmptyBuffer of WindowManager for backgrounds instead, improves performance
                DoNotBringToFront = true,
                TitlebarEnabled = false,
                Title = "Desktop"
            };

            desktopForm.KeyPress += (ev) =>
            {
                if (ev.Key == ConsoleKeyEx.F5)
                {
                    ReloadFiles();
                }
            };

            desktopForm.MouseUp += (state, prev, absoluteX, absoluteY) =>
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

                ImageLabel img = new() {
                    RelativePosX = 5 + 16 + offsetX,
                    RelativePosY = 5 + offsetY,
                    SizeX = 64,
                    SizeY = 64,
                    Image = directoryBmp
                };
                img.SetTransparent(true);
                img.Click += DirectoryClicked;
                fileContainer.AddChild(img);

                Label lbl = new() {
                    RelativePosX = 5 + offsetX,
                    RelativePosY = 69 + offsetY,
                    SizeX = 96,
                    SizeY = 16,
                    Text = directory,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    SafeDrawEnabled = true,
                };

                lbl.Click += DirectoryClicked;
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

                ImageLabel img = new() {
                    RelativePosX = 5 + 16 + offsetX,
                    RelativePosY = 5 + offsetY,
                    SizeX = 64,
                    SizeY = 64,
                    Image = icon
                };
                img.SetTransparent(true);
                img.Click += FileClicked;
                fileContainer.AddChild(img);

                Label lbl = new() {
                    RelativePosX = 5 + offsetX,
                    RelativePosY = 69 + offsetY,
                    SizeX = 96,
                    SizeY = 16,
                    Text = file,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    SafeDrawEnabled = true,
                };

                lbl.Click += FileClicked;
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
            Form form = new(SelfProcess) {
                RelativePosX = 200,
                RelativePosY = 200,

                SizeX = 400,
                SizeY = 130,

                Title = "Rename file",
                TitlebarEnabled = true
            };

            Label lbl = new() {
                RelativePosX = 10,
                RelativePosY = 15,
                SizeX = 390,
                SizeY = 16,
                Text = "Specify a new name for '" + Path.GetFileName(originalPath) + "':",
            };

            TextField newNameField = new() {
                RelativePosX = 10,
                RelativePosY = 35,
                SizeX = 380,
                SizeY = 24,

                Text = (Path.GetFileName(originalPath))
            };

            TextButton cancelBtn = new() {
                RelativePosX = 10,
                RelativePosY = 70,
                SizeX = 185,
                SizeY = 24,
                Text = ("Cancel")
            };
            cancelBtn.Click += (state, x, y) => {
                WindowManager.RemoveForm(form);
            };

            TextButton okBtn = new() {
                RelativePosX = 205,
                RelativePosY = 70,
                SizeX = 185,
                SizeY = 24,
                Text = ("OK")
            };
            okBtn.Click += (state, x, y) => {
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
