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
        }

        public override void Init() {
            loginForm = new();
            loginForm.RelativePosX = 0;
            loginForm.RelativePosY = 0;
            loginForm.SizeX = 600;
            loginForm.SizeY = 400;
            loginForm.SetTitlebarEnabled(true);
            loginForm.SetTitle("Login - nxtlvlOS");

            #region Create container
            Container loginContainer = new();
            loginContainer.RelativePosX = 0;
            loginContainer.RelativePosY = 0;
            #endregion

            #region Create login elements
            Label accountTitle = new();
            accountTitle.SizeX = 500;
            accountTitle.SizeY = 24;
            accountTitle.RelativePosX = 0;
            accountTitle.RelativePosY = 0;
            accountTitle.SetText("Login");
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

            TextButton accountLogin = new();
            accountLogin.SizeX = 500;
            accountLogin.SizeY = 24;
            accountLogin.RelativePosX = 0;
            accountLogin.RelativePosY = 150;
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

            loginContainer.AddElement(accountTitle);
            loginContainer.AddElement(accountUsername);
            loginContainer.AddElement(accountPassword);
            loginContainer.AddElement(accountLogin);

            loginForm.AddElement(loginContainer);
            loginContainer.AdjustToBoundingBox(HorizontalAlignment.Center, VerticalAlignment.Middle);
            #endregion

            WindowManager.AddForm(loginForm);
        }

        public override void Update() {
            
        }
    }
}
