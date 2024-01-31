using Cosmos.HAL;
using nxtlvlOS.Processing;
using nxtlvlOS.Windowing;
using nxtlvlOS.Windowing.Elements;
using nxtlvlOS.Windowing.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Apps {
    public class TaskManager : App {
        private Form taskmgrForm;
        private ListView processList;
        private int lastSecond = -1;

        public override void Exit() {
            if (taskmgrForm != null && !taskmgrForm.IsBeingClosed) WindowManager.RemoveForm(taskmgrForm);
        }

        public override void Init(string[] args) {
            taskmgrForm = new(SelfProcess) {
                SizeX = 600,
                SizeY = 400 + 24, // 22 for titlebar
                RelativePosX = 100,
                RelativePosY = 100,
                Title = "Task Manager - nxtlvlOS",
                TitlebarEnabled = true
            };

            // TODO: Add tabs: Processes, Performance, Details
            // For now we show a simple list of processes
            processList = new() {
                RelativePosX = 25,
                RelativePosY = 25,
                SizeX = 550,
                SizeY = 350,
                BackroundColor = ColorUtils.Primary300,
                ItemColor = ColorUtils.Primary400,
                SelectedItemColor = ColorUtils.Primary200
            };

            taskmgrForm.AddChild(processList);

            taskmgrForm.Closed += () => {
                ProcessManager.KillProcess(SelfProcess);
            };

            WindowManager.AddForm(taskmgrForm);
        }

        public override void Update() {
            if(lastSecond != RTC.Second) {
                Kernel.Instance.Logger.Log(LogLevel.Sill, "Reloading processes. Current selected index: " + processList.SelectedIndex);
                ReloadProcesses();
            }
        }

        private void ReloadProcesses() {
            lastSecond = RTC.Second;

            // Make sure the last selected stays selected
            string lastSelected = (string)processList.SelectedItem;

            // We do not do doLayout to allow buttons to be reused, instead of recreating them
            processList.ClearItems(doLayout: false);
            int idx = 0;
            int idxToSelect = -1;

            foreach(Process p in ProcessManager.Processes) {
                var itemName = p.Name + " (" + p.Pid + ")";
                processList.AddItem(itemName, doLayout: false); // Same reason for doLayout: false as above

                if(lastSelected != null && lastSelected == itemName) {
                    idxToSelect = idx;
                }

                idx++;
            }

            // Manually trigger the layout, now that we added all items
            processList.DoLayout();

            if(idxToSelect != -1) {
                processList.SelectItem(idxToSelect);
            }
        }
    }
}
