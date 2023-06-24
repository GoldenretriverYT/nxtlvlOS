using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Assets {
    public class AssetManager {
        [ManifestResourceStream(ResourceName = "nxtlvlOS.Assets.Cursor.nxtbmp")]
        public static byte[] CursorBmp;

        [ManifestResourceStream(ResourceName = "nxtlvlOS.Assets.FileIconGeneric.nxtbmp")]
        public static byte[] FileIconGeneric;

        public static void IncludeFiles() {
            if (!Kernel.FS.DirectoryExists("/System/")) {
                Kernel.FS.CreateDirectory("/System/");
            }

            if (!Kernel.FS.DirectoryExists("/System/FileExts/")) {
                Kernel.FS.CreateDirectory("/System/FileExts/");
            }

            if (!Kernel.FS.DirectoryExists("/System/FileExts/Assoc/")) {
                Kernel.FS.CreateDirectory("/System/FileExts/Assoc/");
            }


            Kernel.FS.WriteAllText("/System/FileExts/Assoc/asc.asc",
            @"[assoc]
Name=Association File
NativeTarget=NotepadApp
StartArgs={fullpath}");

            Kernel.FS.WriteAllText("/System/FileExts/Assoc/bmp.asc",
            @"[assoc]
Name=Bitmap
NativeTarget=ImageViewerApp
StartArgs={fullpath}");

            Kernel.FS.WriteAllText("/System/FileExts/Assoc/dex.asc",
            @"[assoc]
Name=.NET Executable (dotnetparser)
NativeTarget=DotnetParserRuntimeApp
StartArgs={fullpath} {args}");

            Kernel.FS.WriteAllText("/System/FileExts/Assoc/txt.asc",
            @"[assoc]
Name=Text File
NativeTarget=NotepadApp
StartArgs={fullpath}");
        }
    }
}
