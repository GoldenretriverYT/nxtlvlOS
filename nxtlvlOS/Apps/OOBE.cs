using Cosmos.System;
using Cosmos.System.ScanMaps;
using nxtlvlOS.Services;
using nxtlvlOS.Processing;
using nxtlvlOS.Utils;
using nxtlvlOS.Windowing;
using nxtlvlOS.Windowing.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using nxtlvlOS.Assets;

namespace nxtlvlOS.Apps
{
    public class OOBE : App
    {
        static List<(string, ScanMapBase)> KeyboardLayouts = new() {
            ("English, US", new USStandardLayout()),
            ("German, DE", new DEStandardLayout()),
            ("Spanish, ES", new ESStandardLayout()),
            ("French, FR", new FRStandardLayout()),
            ("Turkish, TR", new TRStandardLayout())
        };

        private Form oobeForm;

        public override void Exit()
        {
            Kernel.Instance.Logger.Log(LogLevel.Info, "Exiting OOBE");
            if (oobeForm != null) oobeForm.Close();
            ProcessManager.CreateProcess(new LoginApp(), "UAC Login");
        }

        public override void Init(string[] args)
        {
            if (!Directory.Exists(@"0:\System"))
            {
                Directory.CreateDirectory(@"0:\System");
            }

            if (File.Exists(@"0:\System\oobedone"))
            {
                if (File.Exists(@"0:\System\kblyt.cfg"))
                { // TODO: Offload this to a KeyboardService
                    var kbLayout = File.ReadAllText(@"0:\System\kblyt.cfg");

                    foreach (var layout in KeyboardLayouts)
                    {
                        if (layout.Item1 == kbLayout)
                        {
                            KeyboardManager.SetKeyLayout(layout.Item2);
                            break;
                        }
                    }
                }

                ProcessManager.KillProcess(SelfProcess);
                return;
            }

            oobeForm = new(SelfProcess);
            oobeForm.RelativePosX = (1280 - 600) / 2;
            oobeForm.RelativePosY = (720 - 400) / 2;
            oobeForm.SizeX = 600;
            oobeForm.SizeY = 400;
            oobeForm.SetTitlebarEnabled(true);

            #region Create step containers here for referencing purposes
            Container stepSelectKeyboardLayout = new();
            stepSelectKeyboardLayout.Visible = true;

            Container stepCreateAccountContainer = new();
            stepCreateAccountContainer.Visible = false;

            Container stepCopyFilesContainer = new();
            stepCopyFilesContainer.Visible = false;
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
            stepSelectKeyboardLayout.AddChild(kbTitle);

            var offset = 50;

            foreach (var kbLayout in KeyboardLayouts)
            {
                var _layout = kbLayout;

                TextButton layoutButton = new();
                layoutButton.SizeX = 500;
                layoutButton.SizeY = 24;
                layoutButton.RelativePosX = 0;
                layoutButton.RelativePosY = offset;
                layoutButton.SetText(kbLayout.Item1);
                layoutButton.SetHorizontalAlignment(HorizontalAlignment.Center);
                layoutButton.SetVerticalAlignment(VerticalAlignment.Middle);

                layoutButton.Click = (state, absoluteX, absoluteY) =>
                {
                    stepSelectKeyboardLayout.Visible = false;
                    stepCreateAccountContainer.Visible = true;

                    KeyboardManager.SetKeyLayout(_layout.Item2); // TODO: Offload this to a KeyboardService
                    File.WriteAllText(@"0:\System\kblyt.cfg", _layout.Item1);
                };

                offset += 30;
                stepSelectKeyboardLayout.AddChild(layoutButton);
            }

            oobeForm.AddChild(stepSelectKeyboardLayout); // IMPORTANT: AdjustToBoundingBox needs a parent first!
            stepSelectKeyboardLayout.AdjustBoundingBoxAndAlignToParent(HorizontalAlignment.Center, VerticalAlignment.Middle);
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
            accountUsername.SetPlaceholder("Username");

            TextField accountPassword = new();
            accountPassword.SizeX = 500;
            accountPassword.SizeY = 24;
            accountPassword.RelativePosX = 0;
            accountPassword.RelativePosY = 100;
            accountPassword.SetPlaceholder("Password");

            TextButton accountNextStep = new();
            accountNextStep.SizeX = 500;
            accountNextStep.SizeY = 24;
            accountNextStep.RelativePosX = 0;
            accountNextStep.RelativePosY = 150;
            accountNextStep.SetText("Next step");
            accountNextStep.SetHorizontalAlignment(HorizontalAlignment.Center);
            accountNextStep.SetVerticalAlignment(VerticalAlignment.Middle);

            accountNextStep.Click = (state, absoluteX, absoluteY) =>
            {
                Kernel.Instance.Logger.Log(LogLevel.Info, "Creating user");
                UACService.Instance.CreateUser(accountUsername.Text, accountPassword.Text);

                stepCreateAccountContainer.Visible = false;
                stepCopyFilesContainer.Visible = true;
            };

            stepCreateAccountContainer.AddChild(accountTitle);
            stepCreateAccountContainer.AddChild(accountUsername);
            stepCreateAccountContainer.AddChild(accountPassword);
            stepCreateAccountContainer.AddChild(accountNextStep);

            oobeForm.AddChild(stepCreateAccountContainer);
            stepCreateAccountContainer.AdjustBoundingBoxAndAlignToParent(HorizontalAlignment.Center, VerticalAlignment.Middle);
            #endregion

            #region Step 3 - Copy files
            Label copyFilesTitle = new();
            copyFilesTitle.SizeX = 500;
            copyFilesTitle.SizeY = 24;
            copyFilesTitle.RelativePosX = 0;
            copyFilesTitle.RelativePosY = 0;
            copyFilesTitle.SetText("We will now have to copy a files. This may take a while.");
            copyFilesTitle.SetHorizontalAlignment(HorizontalAlignment.Center);
            copyFilesTitle.SetVerticalAlignment(VerticalAlignment.Middle);

            TextButton copyFilesButton = new();
            copyFilesButton.SizeX = 500;
            copyFilesButton.SizeY = 24;
            copyFilesButton.RelativePosX = 0;
            copyFilesButton.RelativePosY = 50;
            copyFilesButton.SetText("Copy files & finish OOBE");
            copyFilesButton.SetHorizontalAlignment(HorizontalAlignment.Center);
            copyFilesButton.SetVerticalAlignment(VerticalAlignment.Middle);

            copyFilesButton.Click = (state, absoluteX, absoluteY) =>
            {
                if(!Directory.Exists(@"1:\System\")) {
                    copyFilesTitle.SetText("Please insert the nxtlvlOS installation media and try again.");
                    return;
                }

                DirectoryUtils.RecursiveCopy(@"1:\System\", @"0:\System\");

                File.WriteAllText(@"0:\System\oobedone", "1");
                ProcessManager.KillProcess(SelfProcess);
            };

            stepCopyFilesContainer.AddChild(copyFilesTitle);
            stepCopyFilesContainer.AddChild(copyFilesButton);

            oobeForm.AddChild(stepCopyFilesContainer);
            stepCopyFilesContainer.AdjustBoundingBoxAndAlignToParent(HorizontalAlignment.Center, VerticalAlignment.Middle);
            #endregion

            WindowManager.AddForm(oobeForm);
        }

        public override void Update()
        {

        }
    }
}
