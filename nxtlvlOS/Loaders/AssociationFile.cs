using nxtlvlOS.Utils;
using SimpleINI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Loaders {
    public class AssociationFile {
        public string Name, NativeTarget;
        public string StartArgs = "";
        public string IconPath = @"0:\System\FileExts\Icons\txt.bmp"; // Whilst there is a fallback value, the existence of this file is not guranteed

        public NXTBmp Icon {
            get {
                if (_icon == null) {
                    if (File.Exists(IconPath)) {
                        _icon = new NXTBmp(File.ReadAllBytes(IconPath));
                    } else {
                        _icon = null;
                    }
                }

                return _icon;
            }
        }

        private NXTBmp _icon;

        public static ErrorOr<AssociationFile> LoadFrom(string path) {
            if(!File.Exists(path)) {
                return ErrorOr<AssociationFile>.MakeError("Association file does not exist");
            }

            var rawData = File.ReadAllText(path);
            var iniData = INIParser.Parse(rawData);
            var assoc = new AssociationFile();

            if (iniData.TryGetValue("assoc", "Name", out string name)) {
                assoc.Name = name;
            } else return ErrorOr<AssociationFile>.MakeError("Association file needs a name");

            if (iniData.TryGetValue("assoc", "NativeTarget", out string nativeTarget)) {
                assoc.NativeTarget = nativeTarget;
            } else return ErrorOr<AssociationFile>.MakeError("Association file needs a native target");

            if(iniData.TryGetValue("assoc", "StartArgs", out string startArgs)) {
                assoc.StartArgs = startArgs;
            }

            if (iniData.TryGetValue("assoc", "IconPath", out string iconPath)) {
                assoc.IconPath = iconPath;
            }

            return ErrorOr<AssociationFile>.MakeResult(assoc);
        }

        public void WriteTo(string path) {
            var ini = new INIGroupsHolder();
            ini.ForceSetValue("assoc", "Name", Name);
            ini.ForceSetValue("assoc", "NativeTarget", NativeTarget);
            ini.ForceSetValue("assoc", "StartArgs", StartArgs);
            File.WriteAllText(path, ini.Stringify());
        }
    }
}
