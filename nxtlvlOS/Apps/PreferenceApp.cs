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

namespace nxtlvlOS.Apps
{
    public class PreferenceApp : App
    {
        private Form prefForm;

        public override void Exit() {
            if (prefForm != null) prefForm.Close();
        }

        public override void Init(string[] args)
        {
            prefForm = new(SelfProcess);
            prefForm.RelativePosX = (1280 - 600) / 2;
            prefForm.RelativePosY = (720 - 424) / 2;
            prefForm.SizeX = 600;
            prefForm.SizeY = 424;
            prefForm.SetTitlebarEnabled(true);
            prefForm.SetTitle("Preferences - nxtlvlOS");

            
            WindowManager.AddForm(prefForm);
        }

        public override void Update()
        {

        }
    }
}
