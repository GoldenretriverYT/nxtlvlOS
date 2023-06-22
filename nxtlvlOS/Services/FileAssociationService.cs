using nxtlvlOS.Loaders;
using nxtlvlOS.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Services {
    public class FileAssociationService : App {
        public static FileAssociationService Instance;

        private List<AssociationFile> associationFiles = new();
        private Dictionary<string, Func<App>> nativeAppRegistry = new();

        public override void Exit() {
            throw new Exception("FileAssociationService should not be killed.");
        }

        public override void Init(string[] args) {
            if (Instance != null)
                throw new Exception("FileAssociationService should not be started twice.");

            Instance = this;
        }

        public override void Update() {
        }

        public void RegisterApp(string name, Func<App> factory) {
            nativeAppRegistry.Add(name, factory);
        }
    }
}
