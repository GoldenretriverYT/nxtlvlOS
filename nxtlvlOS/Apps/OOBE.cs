using Cosmos.System;
using Cosmos.System.ScanMaps;
using nxtlvlOS.Processing;
using nxtlvlOS.Windowing;
using nxtlvlOS.Windowing.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Apps {
    public class OOBE : App {
        static List<(string, ScanMapBase)> KeyboardLayouts = new() {
            ("English, US", new USStandardLayout()),
            ("German, DE", new DEStandardLayout()),
            ("Spanish, ES", new ESStandardLayout()),
            ("French, FR", new FRStandardLayout()),
            ("Turkish, TR", new TRStandardLayout())
        };

        public override void Exit() {
            
        }

        public override void Init() {
            if (!Directory.Exists(@"0:\System")) {
                Directory.CreateDirectory(@"0:\System");
            }

            if (File.Exists(@"0:\System\oobedone")) {
                if(File.Exists(@"0:\System\kblyt.cfg")) { // TODO: Offload this to a KeyboardService
                    var kbLayout = File.ReadAllText(@"0:\System\kblyt.cfg");

                    foreach(var layout in KeyboardLayouts) {
                        if(layout.Item1 == kbLayout) {
                            KeyboardManager.SetKeyLayout(layout.Item2);
                            break;
                        }
                    }
                }

                ProcessManager.KillProcess(SelfProcess);
            }

            Form oobeForm = new();
            oobeForm.RelativePosX = 0;
            oobeForm.RelativePosY = 0;
            oobeForm.SizeX = 1280;
            oobeForm.SizeY = 720;
            oobeForm.SetTitlebarEnabled(false);

            #region Create step containers here for referencing purposes
            Container stepSelectKeyboardLayout = new();
            stepSelectKeyboardLayout.RelativePosX = 0;
            stepSelectKeyboardLayout.RelativePosY = 0;
            stepSelectKeyboardLayout.Visible = true;

            Container stepCreateAccountContainer = new();
            stepCreateAccountContainer.RelativePosX = 0;
            stepCreateAccountContainer.RelativePosY = 0;
            stepCreateAccountContainer.Visible = false;
            #endregion

            #region Step 1 - Select a keyboard layout

            Label kbTitle = new();
            kbTitle.SizeX = 500;
            kbTitle.SizeY = 24;
            kbTitle.RelativePosX = 0;
            kbTitle.RelativePosY = 0;
            kbTitle.SetText("Select a keyboard layout to start the OOBE experience!");
            kbTitle.SetHorizontalAlignment(HorizontalAlignment.Center);
            kbTitle.SetVerticalAlignment(VerticalAlignment.Middle);
            stepSelectKeyboardLayout.AddElement(kbTitle);

            var offset = 50;

            foreach (var kbLayout in KeyboardLayouts) {
                var _layout = kbLayout;

                TextButton layoutButton = new();
                layoutButton.SizeX = 500;
                layoutButton.SizeY = 24;
                layoutButton.RelativePosX = 0;
                layoutButton.RelativePosY = offset;
                layoutButton.SetText(kbLayout.Item1);
                layoutButton.SetHorizontalAlignment(HorizontalAlignment.Center);
                layoutButton.SetVerticalAlignment(VerticalAlignment.Middle);

                layoutButton.Click = (MouseState state, uint absoluteX, uint absoluteY) => {
                    stepSelectKeyboardLayout.Visible = false;
                    stepCreateAccountContainer.Visible = true;

                    KeyboardManager.SetKeyLayout(_layout.Item2); // TODO: Offload this to a KeyboardService
                    File.WriteAllText(@"0:\System\kblyt.cfg", _layout.Item1);
                };

                offset += 30;
                stepSelectKeyboardLayout.AddElement(layoutButton);
            }

            oobeForm.AddElement(stepSelectKeyboardLayout); // IMPORTANT: AdjustToBoundingBox needs a parent first!
            stepSelectKeyboardLayout.AdjustToBoundingBox(HorizontalAlignment.Center, VerticalAlignment.Middle);
            #endregion

            #region Step 2 - Create account
            Label accountTitle = new();
            accountTitle.SizeX = 500;
            accountTitle.SizeY = 24;
            accountTitle.RelativePosX = 0;
            accountTitle.RelativePosY = 0;
            accountTitle.SetText("Create an account to start using nxtlvlOS!");
            accountTitle.SetHorizontalAlignment(HorizontalAlignment.Center);
            accountTitle.SetVerticalAlignment(VerticalAlignment.Middle);

            TextField accountUsername = new();
            accountUsername.SizeX = 500;
            accountUsername.SizeY = 24;
            accountUsername.RelativePosX = 0;
            accountUsername.RelativePosY = 50;
            accountUsername.SetText("Username");

            TextField accountPassword = new();
            accountPassword.SizeX = 500;
            accountPassword.SizeY = 24;
            accountPassword.RelativePosX = 0;
            accountPassword.RelativePosY = 100;
            accountPassword.SetText("Password");

            TextButton accountNextStep = new();
            accountNextStep.SizeX = 500;
            accountNextStep.SizeY = 24;
            accountNextStep.RelativePosX = 0;
            accountNextStep.RelativePosY = 150;
            accountNextStep.SetText("Next step");
            accountNextStep.SetHorizontalAlignment(HorizontalAlignment.Center);
            accountNextStep.SetVerticalAlignment(VerticalAlignment.Middle);

            stepCreateAccountContainer.AddElement(accountTitle);
            stepCreateAccountContainer.AddElement(accountUsername);
            stepCreateAccountContainer.AddElement(accountPassword);
            stepCreateAccountContainer.AddElement(accountNextStep);

            oobeForm.AddElement(stepCreateAccountContainer);
            stepCreateAccountContainer.AdjustToBoundingBox(HorizontalAlignment.Center, VerticalAlignment.Middle);
            #endregion

            WindowManager.AddForm(oobeForm);
        }

        public override void Update() {
            
        }
    }
}
