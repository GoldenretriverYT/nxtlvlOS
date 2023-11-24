using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Assets {
    public class AssetManager {
        [ManifestResourceStream(ResourceName = "nxtlvlOS.Assets.Cursor.nxtbmp")]
        public static byte[] CursorBmp;

        [ManifestResourceStream(ResourceName = "nxtlvlOS.Assets.FileIconGeneric.nxtbmp")]
        public static byte[] FileIconGeneric;

        [ManifestResourceStream(ResourceName = "nxtlvlOS.Assets.BackgroundDefault.nxtbmp")]
        public static byte[] DefaultBackground;

        public static void IncludeFiles() {
            if (!Directory.Exists("0:/System/")) {
                Directory.CreateDirectory("0:/System/");
            }

            if (!Directory.Exists("0:/System/FileExts/")) {
                Directory.CreateDirectory("0:/System/FileExts/");
            }

            if (!Directory.Exists("0:/System/FileExts/Assoc/")) {
                Directory.CreateDirectory("0:/System/FileExts/Assoc/");
            }


            File.WriteAllText("0:/System/FileExts/Assoc/asc.asc",
            @"[assoc]
Name=Association File
NativeTarget=NotepadApp
StartArgs={fullpath}");

            File.WriteAllText("0:/System/FileExts/Assoc/bmp.asc",
            @"[assoc]
Name=Bitmap
NativeTarget=ImageViewerApp
StartArgs={fullpath}");

            File.WriteAllText("0:/System/FileExts/Assoc/dex.asc",
            @"[assoc]
Name=.NET Executable (dotnetparser)
NativeTarget=DotnetParserRuntimeApp
StartArgs={fullpath} {args}");

            File.WriteAllText("0:/System/FileExts/Assoc/txt.asc",
            @"[assoc]
Name=Text File
NativeTarget=NotepadApp
StartArgs={fullpath}");
        }
    }
}
