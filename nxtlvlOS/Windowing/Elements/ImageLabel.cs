using nxtlvlOS.Windowing.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Elements {
    public class ImageLabel : BufferedElement {
        private uint[] image = new uint[0];
        public uint[] Image => image;

        public ImageLabel() {
            DrawMode = BufferDrawMode.RawCopy;
        }

        public override void Draw() {
            if (image.Length != SizeX * SizeY) throw new Exception("Image size mismatch. Scaling is not support yet.");

            Buffer = image;
        }

        public void SetImage(uint[] image) {
            if (image.Length != SizeX * SizeY) throw new Exception("Image size mismatch. Scaling is not support yet.");

            this.image = image;
            this.SetDirty(true);
        }

        public void SetTransparent(bool isTransparent) {
            DrawMode = isTransparent ? BufferDrawMode.PixelByPixel : BufferDrawMode.RawCopy;
            this.SetDirty(true);
        }
    }
}
