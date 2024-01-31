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

            oobeForm = new(SelfProcess) {
                RelativePosX = (1280 - 600) / 2,
                RelativePosY = (720 - 400) / 2,
                SizeX = 600,
                SizeY = 400,
                TitlebarEnabled = (true)
            };

            #region Create step containers here for referencing purposes
            Container stepSelectKeyboardLayout = new() {
                Visible = true
            };

            Container stepCreateAccountContainer = new() {
                Visible = false
            };

            Container stepCopyFilesContainer = new() {
                Visible = false
            };
            #endregion

            #region Step 1 - Select a keyboard layout

            Label kbTitle = new() {
                RelativePosX = 0,
                RelativePosY = 0,
                SizeX = 500,
                SizeY = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Middle,
                Text = "Select a keyboard layout to start the OOBE experience!"
            };
            stepSelectKeyboardLayout.AddChild(kbTitle);

            var offset = 50;

            foreach (var kbLayout in KeyboardLayouts)
            {
                var _layout = kbLayout;

                TextButton layoutButton = new() {
                    SizeX = 500,
                    SizeY = 24,
                    RelativePosX = 0,
                    RelativePosY = offset,
                    Text = kbLayout.Item1,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Middle
                };

                layoutButton.Click += (state, absoluteX, absoluteY) =>
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
            Label accountTitle = new() {
                RelativePosX = 0,
                RelativePosY = 0,
                SizeX = 500,
                SizeY = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Middle,
                Text = "Create an account to start using nxtlvlOS!"
            };

            TextField accountUsername = new() {
                SizeX = 500,
                SizeY = 24,
                RelativePosX = 0,
                RelativePosY = 50,
                Placeholder = ("Username")
            };

            TextField accountPassword = new() {
                SizeX = 500,
                SizeY = 24,
                RelativePosX = 0,
                RelativePosY = 100,
                Placeholder = ("Password")
            };

            TextButton accountNextStep = new() {
                SizeX = 500,
                SizeY = 24,
                RelativePosX = 0,
                RelativePosY = 150,
                Text = ("Next step"),
                HorizontalAlignment = (HorizontalAlignment.Center),
                VerticalAlignment = (VerticalAlignment.Middle)
            };

            accountNextStep.Click += (state, absoluteX, absoluteY) =>
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
            Label copyFilesTitle = new() {
                RelativePosX = 0,
                RelativePosY = 0,
                SizeX = 500,
                SizeY = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Middle,
                Text = "We will now have to copy a files. This may take a while."
            };

            TextButton copyFilesButton = new() {
                SizeX = 500,
                SizeY = 24,
                RelativePosX = 0,
                RelativePosY = 50,
                Text = ("Copy files & finish OOBE"),
                HorizontalAlignment = (HorizontalAlignment.Center),
                VerticalAlignment = (VerticalAlignment.Middle)
            };

            copyFilesButton.Click += (state, absoluteX, absoluteY) =>
            {
                if(!Directory.Exists(@"1:\System\")) {
                    copyFilesTitle.Text = "Please insert the nxtlvlOS installation media and try again.";
                    DumpFileTree("1:\\");
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

        static void DumpFileTree(string dir = @"0:\", int pad = 2) {
            if(pad == 2) {
                Kernel.Instance.Logger.Log(LogLevel.Sill, "Dumping file tree:");
            }

            foreach (var file in Directory.GetFiles(dir)) {
                Kernel.Instance.Logger.Log(LogLevel.Sill, new string(' ', pad) + "-" + file);
            }

            foreach (var directory in Directory.GetDirectories(dir)) {
                Kernel.Instance.Logger.Log(LogLevel.Sill, new string(' ', pad) + "-" + directory);
                DumpFileTree(dir + directory + @"\", pad + 2);
            }
        }
        
        public override void Update()
        {

        }
    }
}
