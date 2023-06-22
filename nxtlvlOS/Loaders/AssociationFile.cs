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

        public static ErrorOr<AssociationFile> LoadFrom(string path) {
            if(!Kernel.FS.FileExists(path)) {
                return ErrorOr<AssociationFile>.MakeError("Association file does not exist");
            }

            var rawData = Kernel.FS.ReadAllText(path).Data;
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

            return ErrorOr<AssociationFile>.MakeResult(assoc);
        }

        public void WriteTo(string path) {
            var ini = new INIGroupsHolder();
            ini.ForceSetValue("assoc", "Name", Name);
            ini.ForceSetValue("assoc", "NativeTarget", NativeTarget);
            ini.ForceSetValue("assoc", "StartArgs", StartArgs);
            Kernel.FS.WriteAllText(path, ini.Stringify());
        }
    }
}
