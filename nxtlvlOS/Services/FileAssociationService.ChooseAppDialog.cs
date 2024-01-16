using nxtlvlOS.Windowing.Elements;
using nxtlvlOS.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nxtlvlOS.Windowing.Elements.Shapes;
using nxtlvlOS.Windowing.Utils;
using Cosmos.System;
using nxtlvlOS.Loaders;

namespace nxtlvlOS.Services {
    public partial class FileAssociationService {
        public void ShowChooseAppDialog(string ext, string originalPath, string[] originalArgs) {
            Form form = new(SelfProcess);

            form.RelativePosX = 200;
            form.RelativePosY = 200;

            form.SizeX = 400;
            form.SizeY = 200;

            form.SetTitle("Choose an app to open this file");
            form.SetTitlebarEnabled(true);

            ScrollView sv = new();
            sv.RelativePosX = 10;
            sv.RelativePosY = 5;
            sv.SizeX = 390;
            sv.SizeY = 170;

            form.MouseDown += (MouseState state, uint absX, uint absY) => {
                Kernel.Instance.Logger.Log(LogLevel.Sill, sv.ScrollY + "/" + sv.ContainerSizeY);
            };

            sv.ContainerSizeX = 390 - 32; // Make place for the scrollbar

            int yOff = 0;

            foreach (var nativeApp in nativeAppRegistry) {
                Rect button = new();
                button.RelativePosX = 0;
                button.RelativePosY = yOff;
                button.SizeX = 390 - 32;
                button.SizeY = 40;

                void ChooseApp(MouseState prevState, MouseState state, uint absX, uint absY) {
                    AssociationFile assoc = new();
                    assoc.Name = ext + " file";
                    assoc.NativeTarget = nativeApp.Key;
                    assoc.StartArgs = "{fullpath}";

                    var assocPath = AssocFilesRoot + ext + ".asc";
                    assoc.WriteTo(assocPath);

                    Kernel.Instance.Logger.Log(LogLevel.Sill, "Wrote association file to " + assocPath);
                    
                    // Reload association files
                    associationFiles.Clear();
                    LoadAssocFiles();

                    // Start app
                    StartAppFromPath(originalPath, originalArgs);

                    // Close dialog
                    form.Close();
                }

                button.MouseUp += ChooseApp;
                button.BackgroundColor = ColorUtils.Primary300;

                Label lbl = new() {
                    RelativePosX = 10,
                    RelativePosY = 5,
                    SizeX = 370,
                    SizeY = 50,
                    Text = nativeApp.Key,
                    MousePassThrough = true,
                };

                // TODO: Add icon here when image resizing is implemented

                button.AddChild(lbl);
                sv.AddItem(button);

                yOff += 45;
            }

            sv.ContainerSizeY = (uint)yOff;
            form.AddChild(sv);

            WindowManager.AddForm(form);
        }
    }
}
