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
    }
}
