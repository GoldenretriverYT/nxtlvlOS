using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Processing {
    public class Process {
        public string Name;
        public int Pid;
        public App AttachedApp;
    }

    public static class ProcessManager {
        private static int _pidCounter = 0;

        public static List<Process> Processes = new();

        public static Process CreateProcess(App app, string name) {
            var proc = new Process() {
                AttachedApp = app,
                Pid = _pidCounter++,
                Name = name
            };

            Processes.Add(proc);
            proc.AttachedApp.SelfProcess = proc;
            proc.AttachedApp.Init();
            return proc;
        }

        public static Process? GetProcessByPid(int pid) {
            foreach(var process in Processes) {
                if (process.Pid == pid) return process;
            }

            return null;
        }

        public static bool KillProcess(Process process) {
            if (!Processes.Contains(process)) return false;

            process.AttachedApp.Exit();
            Processes.Remove(process);
            return true;
        }
    }
}
