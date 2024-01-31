using Cosmos.System;
using Cosmos.System.ScanMaps;
using nxtlvlOS.Services;
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
using nxtlvlOS.Windowing.Utils;

namespace nxtlvlOS.Apps
{
    public class LoginApp : App
    {
        private Form loginForm;

        public override void Exit()
        {
            if (loginForm != null) loginForm.Close();
            ProcessManager.CreateProcess(new Bootstrapper(), "Bootstrapper");
        }

        public override void Init(string[] args)
        {
            loginForm = new(SelfProcess) {
                RelativePosX = (1280 - 600) / 2,
                RelativePosY = (720 - 170) / 2,
                SizeX = 600,
                SizeY = 170,
                TitlebarEnabled = (true),
                Title = ("Login - nxtlvlOS")
            };
            loginForm.SetCloseButtonEnabled(false);

            #region Create container
            Container loginInputContainer = new() {
                RelativePosX = 0,
                RelativePosY = 0
            };

            Container actionsContainer = new() {
                RelativePosX = 0,
                RelativePosY = 0
            };
            #endregion

            #region Create login elements
            Label accountTitle = new() {
                SizeX = 400,
                SizeY = 16,
                RelativePosX = 0,
                RelativePosY = 0,
                CustomId = "LoginLabel",
                Text = "Login",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Middle
            };

            TextField accountUsername = new() {
                SizeX = 400,
                SizeY = 24,
                RelativePosX = 0,
                RelativePosY = 30,
                CustomId = "__dbg__TextFieldAccountUsername",
                Placeholder = "Username"
            };

            TextField accountPassword = new() {
                SizeX = 400,
                SizeY = 24,
                RelativePosX = 0,
                RelativePosY = 65,
                Placeholder = "Password"
            };

            TextButton accountLogin = new() {
                SizeX = 150,
                SizeY = 24,
                RelativePosX = 0,
                RelativePosY = 0,
                Text = "Login",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Middle
            };

            accountLogin.Click += (state, absoluteX, absoluteY) =>
            {
                if (UACService.Instance.Authenticate(accountUsername.Text, accountPassword.Text))
                {
                    ProcessManager.KillProcess(SelfProcess);
                }
                else
                {
                    accountTitle.Text = "Authentication failed!";
                }
            };

            loginInputContainer.AddChild(accountTitle);
            loginInputContainer.AddChild(accountUsername);
            loginInputContainer.AddChild(accountPassword);
            actionsContainer.AddChild(accountLogin);

            loginForm.AddChild(loginInputContainer);
            loginForm.AddChild(actionsContainer);

            loginInputContainer.AdjustBoundingBoxAndAlignToParent(HorizontalAlignment.Left, VerticalAlignment.Top, 10, 10);
            actionsContainer.AdjustBoundingBoxAndAlignToParent(HorizontalAlignment.Right, VerticalAlignment.Top, 10, 10);

            #endregion

            WindowManager.AddForm(loginForm);
        }

        public override void Update()
        {

        }
    }
}
