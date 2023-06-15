using Cosmos.Core;
using Cosmos.HAL;
using nxtlvlOS.Processing;
using nxtlvlOS.Windowing;
using nxtlvlOS.Windowing.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Apps {
    public class TaskBar : App {
        private Form taskbarForm;
        private Container tasksContainer;

        private int frames = 0;

        public override void Exit() {
            if (taskbarForm != null) WindowManager.RemoveForm(taskbarForm);
        }

        public override void Init() {
            taskbarForm = new Form(SelfProcess);
            taskbarForm.RelativePosX = 0;
            taskbarForm.RelativePosY = 690;
            taskbarForm.SizeX = 1280;
            taskbarForm.SizeY = 30;
            taskbarForm.SetTitlebarEnabled(false);
            taskbarForm.SetTitle("Taskbar");

            tasksContainer = new();
            taskbarForm.AddElement(tasksContainer);

            UpdateTasks();

            WindowManager.AddForm(taskbarForm);
        }

        public override void Update() {
            frames++;

            if(frames > 10) {
                UpdateTasks();
                frames = 0;
            }
        }

        public void UpdateTasks() {
            foreach (var child in tasksContainer.Children.ToList()) {
                tasksContainer.RemoveElement(child);
                GCImplementation.Free(child); // i am not sure why we need this...
            }

            var xOffset = 4;

            foreach(var formElement in WindowManager.Forms) {
                if (formElement is not Form form) continue;

                var btn = new TextButton();
                btn.RelativePosX = xOffset;
                btn.RelativePosY = 3;
                btn.SizeX = 144;
                btn.SizeY = 24;
                btn.SetText(form.Title);
                btn.SetSafeDrawEnabled(true);

                tasksContainer.AddElement(btn);
                xOffset += 100;
            }
        }
    }
}
