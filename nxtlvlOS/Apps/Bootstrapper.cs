﻿using nxtlvlOS.Processing;
using nxtlvlOS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Apps {
    public class Bootstrapper : App {
        public override void Exit() {
            
        }

        public override void Init(string[] args) {
            ProcessManager.CreateProcess(new FileAssociationService(), "FileAssocService");

            ProcessManager.CreateProcess(new Desktop(), "Desktop");
            ProcessManager.CreateProcess(new TaskBar(), "TaskBar");

            ProcessManager.KillProcess(SelfProcess);
        }

        public override void Update() {

        }
    }
}
