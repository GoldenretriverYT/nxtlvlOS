using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Processing {
    public abstract class App {
        public AppType Type;
        public Process SelfProcess;

        public abstract void Init();
        public abstract void Update();
        public abstract void Exit();
    }

    public enum AppType {
        App,
        Service
    }
}
