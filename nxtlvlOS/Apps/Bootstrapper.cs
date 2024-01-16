using Cosmos.Debug.Kernel;
using nxtlvlOS.Processing;
using nxtlvlOS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Apps {
    public class Bootstrapper : App {
        Dictionary<string, string> ye;
        public override void Exit() {
            
        }

        public override void Init(string[] args) {
            try {
                ProcessManager.CreateProcess(new FileAssociationService(), "FileAssocService"); 

                ProcessManager.CreateProcess(new Desktop(), "Desktop");
                ProcessManager.CreateProcess(new TaskBar(), "TaskBar");

                FileAssociationService.Instance.RegisterApp("NotepadApp", @"0:\System\FileExts\Icons\txt.bmp", () => new NotepadApp());

                // Test for scrolling in the choose app dialog
                FileAssociationService.Instance.RegisterApp("NotepadApp2", @"0:\System\FileExts\Icons\txt.bmp", () => new NotepadApp());
                FileAssociationService.Instance.RegisterApp("NotepadApp3", @"0:\System\FileExts\Icons\txt.bmp", () => new NotepadApp());
                FileAssociationService.Instance.RegisterApp("NotepadApp4", @"0:\System\FileExts\Icons\txt.bmp", () => new NotepadApp());
                FileAssociationService.Instance.RegisterApp("NotepadApp5", @"0:\System\FileExts\Icons\txt.bmp", () => new NotepadApp());

                ProcessManager.KillProcess(SelfProcess);
            }catch(Exception ex) {
                Kernel.Instance.Panic("Bootstrapping failed: " + ex.Message);
            }
        }

        public override void Update() {

        }
    }
}
