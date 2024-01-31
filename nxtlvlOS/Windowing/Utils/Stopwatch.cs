using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Utils {
    public class Stopwatch {
        public static Stopwatch StartNew() {
            return new Stopwatch();
        }

        private ulong start = 0;
        private ulong stop = 0;
        private bool running = false;

        public Stopwatch() { }

        public void Start() {
            start = Kernel.Instance.MsSinceBoot;
            running = true;
        }

        public void Stop() {
            stop = Kernel.Instance.MsSinceBoot;
            running = false;
        }

        public ulong ElapsedMilliseconds {
            get {
                if (running) {
                    return Kernel.Instance.MsSinceBoot - start;
                } else {
                    return stop - start;
                }
            }
        }

        public void Reset() {
            start = 0;
            stop = 0;
            running = false;
        }

        public void Restart() {
            Reset();
            Start();
        }

        public override string ToString() {
            return $"{ElapsedMilliseconds}ms (start: {start}, stop: {stop})";
        }
    }

    public static class Timings {
        private static Dictionary<string, Stopwatch> _stopwatches = new();

        public static void MeasureStart(string name) {
            if (!_stopwatches.ContainsKey(name)) {
                _stopwatches.Add(name, new Stopwatch());
            }

            _stopwatches[name].Restart();
        }

        public static void MeasureStop(string name) {
            _stopwatches[name].Stop();

            Kernel.Instance.Logger.Log(LogLevel.Info, $"Measured {name} - took {_stopwatches[name]}");
        }
    }
}
