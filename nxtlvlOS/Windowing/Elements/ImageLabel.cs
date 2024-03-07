using nxtlvlOS.Loaders;
using nxtlvlOS.Windowing.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Elements {
    public class ImageLabel : BufferedElement {
        private NXTBmp image;
        public NXTBmp Image {
            get => image;
            set {
                image = value;
                this.SetDirty(true);
            }
        }

        public ImageLabel() {
            DrawMode = BufferDrawMode.RawCopy;
        }

        public override void Draw() {
            SetDirty(false);

            Buffer = image.Data;
        }

        public void SetTransparent(bool isTransparent) {
            DrawMode = isTransparent ? BufferDrawMode.PixelByPixel : BufferDrawMode.RawCopy;
            this.SetDirty(true);
        }
    }
}
