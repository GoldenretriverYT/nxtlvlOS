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
    public class LoginApp : App {
        private Form loginForm;

        public override void Exit() {
            if(loginForm != null) WindowManager.RemoveForm(loginForm);
            ProcessManager.CreateProcess(new Bootstrapper(), "Bootstrapper");
        }

        public override void Init() {
            loginForm = new(SelfProcess);
            loginForm.RelativePosX = (1280-600)/2;
            loginForm.RelativePosY = (720-170)/2;
            loginForm.SizeX = 600;
            loginForm.SizeY = 170;
            loginForm.SetTitlebarEnabled(true);
            loginForm.SetTitle("Login - nxtlvlOS");

            #region Create container
            Container loginInputContainer = new();
            loginInputContainer.RelativePosX = 0;
            loginInputContainer.RelativePosY = 0;

            Container actionsContainer = new();
            actionsContainer.RelativePosX = 0;
            actionsContainer.RelativePosY = 0;
            #endregion

            #region Create login elements
            Label accountTitle = new();
            accountTitle.SizeX = 400;
            accountTitle.SizeY = 16;
            accountTitle.RelativePosX = 0;
            accountTitle.RelativePosY = 0;
            accountTitle.SetText("Login");
            accountTitle.SetHorizontalAlignment(HorizontalAlignment.Left);
            accountTitle.SetVerticalAlignment(VerticalAlignment.Middle);

            TextField accountUsername = new();
            accountUsername.SizeX = 400;
            accountUsername.SizeY = 24;
            accountUsername.RelativePosX = 0;
            accountUsername.RelativePosY = 30;
            accountUsername.SetPlaceholder("Username");

            TextField accountPassword = new();
            accountPassword.SizeX = 400;
            accountPassword.SizeY = 24;
            accountPassword.RelativePosX = 0;
            accountPassword.RelativePosY = 65;
            accountPassword.SetPlaceholder("Password");

            TextButton accountLogin = new();
            accountLogin.SizeX = 150;
            accountLogin.SizeY = 24;
            accountLogin.RelativePosX = 0;
            accountLogin.RelativePosY = 0;
            accountLogin.SetText("Login");
            accountLogin.SetHorizontalAlignment(HorizontalAlignment.Center);
            accountLogin.SetVerticalAlignment(VerticalAlignment.Middle);

            accountLogin.Click = (MouseState state, uint absoluteX, uint absoluteY) => {
                if(UACService.Instance.Authenticate(accountUsername.Text, accountPassword.Text)) {
                    ProcessManager.KillProcess(SelfProcess);
                }else {
                    accountTitle.SetText("Authentication failed!");
                }
            };

            loginInputContainer.AddElement(accountTitle);
            loginInputContainer.AddElement(accountUsername);
            loginInputContainer.AddElement(accountPassword);
            actionsContainer.AddElement(accountLogin);

            loginForm.AddElement(loginInputContainer);
            loginForm.AddElement(actionsContainer);

            loginInputContainer.AdjustToBoundingBox(HorizontalAlignment.Left, VerticalAlignment.Top, 10, 30 + 10); // we have to add 30 to the y padding due to the title bar
            actionsContainer.AdjustToBoundingBox(HorizontalAlignment.Right, VerticalAlignment.Top, 10, 30 + 10);

            #endregion

            WindowManager.AddForm(loginForm);
        }

        public override void Update() {
            
        }
    }
}
