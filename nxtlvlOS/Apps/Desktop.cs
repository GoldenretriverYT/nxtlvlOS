using Cosmos.Core;
using Cosmos.HAL;
using Cosmos.System;
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

namespace nxtlvlOS.Apps {
    public class Desktop : App {
        private Form desktopForm;
        private Container fileContainer;

        private NXTBmp iconBmp = new(Assets.AssetManager.FileIconGeneric); // TODO: Move icon to some FileExtensionService that provides various info about an extension

        private string desktopDir => UACService.UserDir + @"Desktop\";
        private int previousSecond = -1;

        public override void Exit() {
            if(desktopForm != null) WindowManager.RemoveForm(desktopForm);
        }

        public override void Init() {
            desktopForm = new Form(SelfProcess);
            desktopForm.RelativePosX = 0;
            desktopForm.RelativePosY = 0;
            desktopForm.SizeX = 1280;
            desktopForm.SizeY = 720;
            desktopForm.ShouldBeDrawnToScreen = false; // We will use the EmptyBuffer of WindowManager for backgrounds instead, improves performance
            desktopForm.SetTitlebarEnabled(false);
            desktopForm.SetTitle("Desktop");

            desktopForm.KeyPress = (KeyEvent ev) => {
                if (ev.Key == ConsoleKeyEx.F5) {
                    ReloadFiles();
                }
            };

            desktopForm.MouseUp = (MouseState state, MouseState prev, uint absoluteX, uint absoluteY) => {
                Kernel.Instance.Logger.Log(LogLevel.Info, "Desktop mouse up " + (int)(prev & MouseState.Right));
                if ((prev & MouseState.Right) == MouseState.Right) {
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
            desktopForm.AddElement(fileContainer);

            if(!Directory.Exists(desktopDir)) {
                Directory.CreateDirectory(desktopDir);
            }

            ReloadFiles();

            WindowManager.AddForm(desktopForm);
        }

        public override void Update() {
            // Listing files is super slow, lets just skip that and only do it on user request
            //if(RTC.Second % 5 == 0) {
            //    ReloadFiles();
            //}
        }

        public void CreateNewFile() {
            string[] alreadyExistingFiles = Directory.GetFiles(desktopDir);

            string newName = "NewFile";
            int i = 0;

            while (alreadyExistingFiles.Contains(newName)) {
                newName = "NewFile" + i++;
            }

            File.Create(desktopDir + newName).Close();
        }

        public void CreateNewDirectory() {
            string[] alreadyExistingDirs = Directory.GetDirectories(desktopDir);

            string newName = "NewDir";
            int i = 0;

            while(alreadyExistingDirs.Contains(newName)) {
                newName = "NewDir" + i++;
            }

            Directory.CreateDirectory(desktopDir + newName);
        }


        public void ReloadFiles() {
            foreach(var child in fileContainer.Children.ToList()) {
                fileContainer.RemoveElement(child);
            }

            int offsetX = 0, offsetY = 0;

            foreach(var file in Directory.GetFiles(desktopDir)) {
                ImageLabel img = new();
                img.RelativePosX = 5 + 16 + offsetX;
                img.RelativePosY = 5 + offsetY;
                img.SizeX = 64;
                img.SizeY = 64;
                img.SetTransparent(true);
                img.SetImage(iconBmp.Data);
                fileContainer.AddElement(img);

                Label lbl = new();
                lbl.RelativePosX = 5 + offsetX;
                lbl.RelativePosY = 69 + offsetY;
                lbl.SizeX = 96;
                lbl.SizeY = 16;
                lbl.SetText(file);
                lbl.SetHorizontalAlignment(HorizontalAlignment.Center);
                lbl.SetSafeDrawEnabled(true);
                fileContainer.AddElement(lbl);

                offsetY += 80;

                if(offsetY > 480) {
                    offsetY = 0;
                    offsetX += 100;
                }
            }
        }
    }
}
