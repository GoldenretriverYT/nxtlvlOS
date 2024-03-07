using IL2CPU.API.Attribs;
using nxtlvlOS.Loaders;
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

        private static Dictionary<string, NXTBmp> imageCache = new Dictionary<string, NXTBmp>();
        
        public static NXTBmp LoadDynamicImage(string path, ImageType imageType = ImageType.BMP, ImageLoadFallbackStrategy fallbackStrategy = ImageLoadFallbackStrategy.EmptyImage) {
            if(imageCache.ContainsKey(path)) {
                return imageCache[path];
            }

            if (File.Exists(path)) {
                NXTBmp bmp;

                if (imageType == ImageType.BMP) {
                    bmp = NXTBmp.FromBitmap(new Cosmos.System.Graphics.Bitmap(path));
                } else if (imageType == ImageType.NXTBMP) {
                    bmp = new NXTBmp(File.ReadAllBytes(path));
                } else throw new ArgumentOutOfRangeException("imageType", "Provided image type is not supported.");

                imageCache.Add(path, bmp);
                return bmp;
            } else {
                switch (fallbackStrategy) {
                    case ImageLoadFallbackStrategy.ThrowException:
                        throw new FileNotFoundException("File not found: " + path);
                    case ImageLoadFallbackStrategy.EmptyImage:
                        var bmp = new NXTBmp(new byte[8] { 1, 0, 1, 0, 0, 0, 0, 0 }); // 1x1 image
                        return bmp;
                    default:
                        throw new ArgumentOutOfRangeException("fallbackStrategy", "Unrecognized fallback strategy.");
                }
            }
        }
    }

    public enum ImageLoadFallbackStrategy {
        ThrowException,
        EmptyImage
    }

    public enum ImageType {
        BMP, // We use Cosmos BMP parser/reader and copy the data to our own format
        NXTBMP, // A custom, compact format only having the size + raw data
    }
}
