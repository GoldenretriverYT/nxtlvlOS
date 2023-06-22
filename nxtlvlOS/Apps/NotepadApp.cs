using nxtlvlOS.Processing;
using nxtlvlOS.Windowing.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Apps
{
    public class NotepadApp : App {
        private Form form;

        public override void Exit() {
            
        }

        public override void Init(string[] args) {
            form = new(SelfProcess);
            form.RelativePosX = 100;
            form.RelativePosY = 100;

            form.SizeX = 600;
            form.SizeY = 400;

            form.SetTitle("Notepad");
            form.SetTitlebarEnabled(true);
        }

        public override void Update() {

        }
    }
}
