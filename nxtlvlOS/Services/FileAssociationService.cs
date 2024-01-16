using nxtlvlOS.Loaders;
using nxtlvlOS.Processing;
using nxtlvlOS.Utils;
using nxtlvlOS.Windowing;
using nxtlvlOS.Windowing.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Services {
    public partial class FileAssociationService : App {
        public static FileAssociationService Instance;

        private Dictionary<string, AssociationFile> associationFiles = new();
        private Dictionary<string, NativeAppRegistryEntry> nativeAppRegistry = new();

        private const string AssocFilesRoot = @"0:\System\FileExts\Assoc\";

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
            Kernel.Instance.Logger.Log(LogLevel.Sill, "Opening file " + path);
            var extension = Path.GetExtension(path).Substring(1);
            
            if (!associationFiles.ContainsKey(extension)) {
                Kernel.Instance.Logger.Log(LogLevel.Sill, "No suitable association found! Extension: " + extension
                    + "Known Extensions: " + string.Join(", ", associationFiles.Keys));

                ShowChooseAppDialog(extension, path, args);
                return;
            }

            var association = associationFiles[extension];
            StartAppFromAssociation(association, path, args);
        }

        public void StartAppFromAssociation(AssociationFile assocFile, string path, string[] args) {
            if (!nativeAppRegistry.ContainsKey(assocFile.NativeTarget)) {
                return; // TODO: Implement "Broken Association" dialog
            }

            var nativeApp = nativeAppRegistry[assocFile.NativeTarget];
            var app = nativeApp.appFactory();

            string[] startArgs = assocFile.StartArgs.Split(' ');
            List<string> finalStartArgs = new();

            foreach (var str in startArgs) {
                if (str == "{fullpath}") {
                    finalStartArgs.Add(path);
                } else if (str == "{args}") {
                    foreach (var arg in args) finalStartArgs.Add(arg);
                } else {
                    finalStartArgs.Add(str);
                }
            }

            ProcessManager.CreateProcess(app, assocFile.Name, finalStartArgs.ToArray());
        }

        public void RegisterApp(string name, string iconPath, Func<App> factory) {
            nativeAppRegistry.Add(name, new(iconPath, factory));
        }

        private void LoadAssocFiles() {
            foreach (var file in Directory.GetFiles(AssocFilesRoot)) {
                Kernel.Instance.Logger.Log(LogLevel.Sill, "Trying to load association file " + file);
                ErrorOr<AssociationFile> result = AssociationFile.LoadFrom(AssocFilesRoot + file);

                if (result.IsError) {
                    Kernel.Instance.Logger.Log(LogLevel.Fail, "Was not able to load association file " + file + ": " + result.Error);
                    continue;
                }

                associationFiles.Add(Path.GetFileNameWithoutExtension(file), result.Data);
            }
        }

        public void ReloadAssocFiles() {
            associationFiles.Clear();
            LoadAssocFiles();
        }

        public ErrorOr<AssociationFile> GetAssocFromPath(string path) {
            return GetAssocFromExt(Path.GetExtension(path).Substring(1));
        }

        public ErrorOr<AssociationFile> GetAssocFromExt(string ext) {
            if (associationFiles.ContainsKey(ext)) {
                return ErrorOr<AssociationFile>.MakeResult(associationFiles[ext]);
            }

            return ErrorOr<AssociationFile>.MakeError("No association file found for extension " + ext);
        }
    }

    internal record struct NativeAppRegistryEntry(string iconPath, Func<App> appFactory) {
        public static implicit operator (string iconPath, Func<App> appFactory)(NativeAppRegistryEntry value) {
            return (value.iconPath, value.appFactory);
        }

        public static implicit operator NativeAppRegistryEntry((string iconPath, Func<App> appFactory) value) {
            return new NativeAppRegistryEntry(value.iconPath, value.appFactory);
        }
    }
}
