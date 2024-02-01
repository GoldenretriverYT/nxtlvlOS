using Cosmos.Core;
using Cosmos.HAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Global = Cosmos.HAL.Global;

namespace nxtlvlOS.Utils {
    public class TimingUtils {
        public static Dictionary<string, ulong> Timings = new();


        public static void Init() {
        }

        public static void Time(string key) {
            Timings.Add(key, CPU.GetCPUUptime());
        }

        public static ulong EndTime(string key) {
            if (!Timings.ContainsKey(key)) {
                Kernel.Instance.mDebugger.Send("[WARN] Tried to end time for key " + key + " but it doesn't exist!");
                return 1337;
            }

            ulong time = CPU.GetCPUUptime() - Timings[key];
            //Kernel.Instance.mDebugger.Send("[INFO] " + key + " took " + time + " CPU cycles (" + CyclestoMS(time) + "ms)");
            Timings.Remove(key);

            return time;
        }
    }
}
