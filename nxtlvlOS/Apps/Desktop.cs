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

        public override void Exit() {
            WindowManager.RemoveForm(desktopForm);
        }

        public override void Init() {


            desktopForm = new Form();
            desktopForm.RelativePosX = 0;
            desktopForm.RelativePosY = 0;
            desktopForm.SizeX = 1280;
            desktopForm.SizeY = 720;
            desktopForm.SetTitlebarEnabled(false);

            WindowManager.AddForm(desktopForm);
        }

        public override void Update() {
            
        }
    }
}
