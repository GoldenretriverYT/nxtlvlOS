using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Utils {
    public class DirectoryUtils {
        public static void RecursiveCopy(string src, string dest, bool overwrite = true) {
            if (src[src.Length - 1] != '\\') {
                src = src += "\\";
            }

            if (dest[dest.Length - 1] != '\\') {
                dest = dest += "\\";
            }

            foreach (var file in Directory.GetFiles(src)) {
                try {
                    Kernel.Instance.Logger.Log(LogLevel.Info, "Copying (file) " + (src + file) + " to " + (dest + file));
                    FileUtils.SafeCopy(src + file, dest + file);
                }catch(Exception ex) {
                    Kernel.Instance.Logger.Log(LogLevel.Fail, "Failed copying file: " + ex.Message);
                }
            }

            foreach(var dir in Directory.GetDirectories(src)) {
                Kernel.Instance.Logger.Log(LogLevel.Info, "Copying (dir) " + (src + dir) + " to " + (dest + dir));

                if(!Directory.Exists(dest + dir)) {
                    Directory.CreateDirectory(dest + dir);
                }
                RecursiveCopy(src + dir + "\\", dest + dir + "\\");
            }
        }
    }
}
