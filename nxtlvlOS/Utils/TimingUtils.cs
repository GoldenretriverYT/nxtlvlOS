using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Utils {
    public class TimingUtils {
        public static Dictionary<string, ulong> Timings = new();

        public static void Time(string key) {
            Timings.Add(key, Cosmos.Core.CPU.GetCPUUptime());
        }

        public static ulong EndTime(string key) {
            if (!Timings.ContainsKey(key)) {
                Kernel.Instance.mDebugger.Send("[WARN] Tried to end time for key " + key + " but it doesn't exist!");
                return 1337;
            }

            ulong time = Cosmos.Core.CPU.GetCPUUptime() - Timings[key];
            //Kernel.Instance.mDebugger.Send("[INFO] " + key + " took " + (time / 1_000_000) + "ms");
            Timings.Remove(key);

            return time;
        }
    }
}
