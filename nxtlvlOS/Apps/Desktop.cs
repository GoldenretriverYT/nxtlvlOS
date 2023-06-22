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
using nxtlvlOS.RAMFS;

namespace nxtlvlOS.Apps
{
    public class Desktop : App
    {
        private Form desktopForm;
        private Container fileContainer;

        private NXTBmp iconBmp = new(Assets.AssetManager.FileIconGeneric); // TODO: Move icon to some FileExtensionService that provides various info about an extension

        private string desktopDir => UACService.UserDir + @"Desktop/";
        private int previousSecond = -1;

        public override void Exit()
        {
            if (desktopForm != null) WindowManager.RemoveForm(desktopForm);
        }

        public override void Init(string[] args)
        {
            desktopForm = new Form(SelfProcess);
            desktopForm.RelativePosX = 0;
            desktopForm.RelativePosY = 0;
            desktopForm.SizeX = 1280;
            desktopForm.SizeY = 720;
            desktopForm.ShouldBeDrawnToScreen = false; // We will use the EmptyBuffer of WindowManager for backgrounds instead, improves performance
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

            if (!Kernel.FS.DirectoryExists(desktopDir))
            {
                Kernel.Instance.Logger.Log(LogLevel.Info, "Creating missing desktop dir");
                var res = Kernel.FS.CreateDirectory(desktopDir);

                if(res.IsError) {
                    Kernel.Instance.Panic("Was not able to create desktop directory: " + res.Error);
                }
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
            string[] alreadyExistingFiles = Kernel.FS.GetFiles(desktopDir).Data.GetNames();

            string newName = "New.txt";
            int i = 0;

            while (alreadyExistingFiles.Contains(newName))
            {
                newName = "New " + i++ + ".txt";
            }

            Kernel.FS.WriteAllText(desktopDir + newName, "");
        }

        public void CreateNewDirectory()
        {
            var alreadyExistingDirs = Kernel.FS.GetDirectories(desktopDir).Data.GetNames();

            string newName = "New";
            int i = 0;

            while (alreadyExistingDirs.Contains(newName))
            {
                newName = "New " + i++;
            }

            Kernel.FS.CreateDirectory(desktopDir + newName);
        }


        public void ReloadFiles()
        {
            foreach (var child in fileContainer.Children.ToList())
            {
                fileContainer.RemoveElement(child);
            }

            int offsetX = 0, offsetY = 0;

            foreach (var file in Kernel.FS.GetFiles(desktopDir).Data.GetNames())
            {
                void FileClicked(MouseState state, uint absX, uint absY) {
                    var pathInfo = FileSystem.ParsePath(file);

                    
                }

                ImageLabel img = new();
                img.RelativePosX = 5 + 16 + offsetX;
                img.RelativePosY = 5 + offsetY;
                img.SizeX = 64;
                img.SizeY = 64;
                img.SetTransparent(true);
                img.SetImage(iconBmp.Data);
                fileContainer.AddChild(img);

                Label lbl = new();
                lbl.RelativePosX = 5 + offsetX;
                lbl.RelativePosY = 69 + offsetY;
                lbl.SizeX = 96;
                lbl.SizeY = 16;
                lbl.SetText(file);
                lbl.SetHorizontalAlignment(HorizontalAlignment.Center);
                lbl.SetSafeDrawEnabled(true);
                fileContainer.AddChild(lbl);

                offsetY += 80;

                if (offsetY > 480)
                {
                    offsetY = 0;
                    offsetX += 100;
                }
            }
        }
    }
}
