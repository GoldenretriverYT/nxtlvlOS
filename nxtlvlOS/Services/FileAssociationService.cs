using nxtlvlOS.Loaders;
using nxtlvlOS.Processing;
using nxtlvlOS.RAMFS;
using nxtlvlOS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Services {
    public class FileAssociationService : App {
        public static FileAssociationService Instance;

        private Dictionary<string, AssociationFile> associationFiles = new();
        private Dictionary<string, Func<App>> nativeAppRegistry = new();

        private const string AssocFilesRoot = @"/System/FileExts/Assoc/";

        public override void Exit() {
            throw new Exception("FileAssociationService should not be killed.");
        }

        public override void Init(string[] args) {
            if (Instance != null)
                throw new Exception("FileAssociationService should not be started twice.");

            Instance = this;

            LoadAssocFiles();
        }

        public override void Update() {
        }

        public void StartAppFromPath(string path, string[] args) {
            PathInfo pathInfo = FileSystem.ParsePath(path);

            if(!associationFiles.ContainsKey(pathInfo.Extension)) {
                return; // TODO: Implement "Choose an app to open this file" dialog
            }

            var association = associationFiles[pathInfo.Extension];
            StartAppFromAssociation(association, path, args);
        }

        public void StartAppFromAssociation(AssociationFile assocFile, string path, string[] args) {
            if (!nativeAppRegistry.ContainsKey(assocFile.NativeTarget)) {
                return; // TODO: Implement "Broken Association" dialog
            }

            var appFactory = nativeAppRegistry[assocFile.NativeTarget];
            var app = appFactory();

            string[] startArgs = assocFile.StartArgs.Split(' ');
            List<string> finalStartArgs = new();

            foreach(var str in startArgs) {
                if(str == "{fullpath}") {
                    finalStartArgs.Add(path);
                }else if(str == "{args}") {
                    foreach (var arg in args) finalStartArgs.Add(arg);
                }else {
                    finalStartArgs.Add(str);
                }
            }

            ProcessManager.CreateProcess(app, assocFile.Name, finalStartArgs.ToArray());
        }

        public void RegisterApp(string name, Func<App> factory) {
            nativeAppRegistry.Add(name, factory);
        }

        private void LoadAssocFiles() {
            foreach(var file in Kernel.FS.GetFiles(AssocFilesRoot).Data.GetNames()) {
                ErrorOr<AssociationFile> result = AssociationFile.LoadFrom(AssocFilesRoot + file);
                PathInfo fileNameInfo = FileSystem.ParsePath(file);

                if(result.IsError) {
                    Kernel.Instance.Logger.Log(LogLevel.Fail, "Was not able to load association file " + file + ": " + result.Error);
                    continue;
                }

                associationFiles.Add(fileNameInfo.Name, result.Data);
            }
        }
    }
}
