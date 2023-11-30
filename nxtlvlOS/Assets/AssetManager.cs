using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Assets {
    public class AssetManager {
        // Default Icon, always included as a fallback
        [ManifestResourceStream(ResourceName = "nxtlvlOS.Assets.FileIconGeneric.nxtbmp")]
        public static byte[] FileIconGeneric;
        
        // TODO: Make cursor load from file system!
        [ManifestResourceStream(ResourceName = "nxtlvlOS.Assets.Cursor.nxtbmp")]
        public static byte[] CursorBmp;
    }
}
