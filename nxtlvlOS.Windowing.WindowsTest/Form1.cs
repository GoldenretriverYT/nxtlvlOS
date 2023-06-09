using SkiaSharp;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace nxtlvlOS.Windowing.WindowsTest {
    public partial class Form1 : Form {
        public static SKSurface surface;
        public static SKImageInfo imageInfo;

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            imageInfo = new SKImageInfo(
                width: pictureBox1.Width,
                height: pictureBox1.Height,
                colorType: SKColorType.Rgba8888,
                alphaType: SKAlphaType.Premul);

            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;

            surface = SKSurface.Create(imageInfo);

            SkiaRenderTarget.canvas = surface.Canvas;

            WindowManager.Target = new SkiaRenderTarget();
            WindowManager.Init();

            RainbowBgTest();
            //SimpleTextTest();
        }

        #region testing stuff
        private void RainbowBgTest() {
            var f1 = new nxtlvlOS.Windowing.Elements.Form();
            f1.RelativePosX = 0;
            f1.RelativePosY = 0;
            f1.SizeX = 1280;
            f1.SizeY = 720;
            f1.SetTitlebarEnabled(true);
            f1.SetTitle("Wow, Form!");
            f1.SetBackgroundColor(0xFFFF0000);
            WindowManager.AddForm(f1);

            for (var x = 0; x < 10; x++) {
                var form = new nxtlvlOS.Windowing.Elements.Form();
                form.RelativePosX = (50 + (x * 50));
                form.RelativePosY = (50 + (x * 50));
                form.SizeX = 200;
                form.SizeY = 200;
                form.SetTitlebarEnabled(true);
                form.SetTitle("Wow, Form! " + x);
                form.SetBackgroundColor(0x80DEDEDE);
                form.DrawMode = BufferDrawMode.PixelByPixel;

                if (x == 1) {
                    var toRight = true;
                    form.PreDrawAndChildUpdate = () => {
                        form.RelativePosX += (toRight ? 3 : -3);
                        if (form.RelativePosX > 600) toRight = false;
                        if (form.RelativePosX < 40) toRight = true;
                    };
                }

                WindowManager.AddForm(form);
            }

            Random rnd = new();
            HSL hsl = new();
            hsl.H = 0;
            hsl.S = 1f;
            hsl.L = 0.5f;

            f1.PostDrawAndChildUpdate = () => {
                var rgb = HSLToRGB(hsl);
                f1.SetBackgroundColor((uint)((0xFF << 24) + (rgb.R << 16) + (rgb.G << 8) + (rgb.B << 0)));
                hsl.H += 1;
                if (hsl.H == 360) hsl.H = 0;
            };
        }
        
        private void SimpleTextTest() {
            var f1 = new nxtlvlOS.Windowing.Elements.Form {
                RelativePosX = 0,
                RelativePosY = 0,
                SizeX = 1280,
                SizeY = 720
            };
            f1.SetTitlebarEnabled(false);
            f1.SetBackgroundColor(0xFF000000);

            var lbl = new nxtlvlOS.Windowing.Elements.Label {
                RelativePosX = 50,
                RelativePosY = 50,
                SizeX = 100,
                SizeY = 30,
            };
            lbl.SetColor(0xFFFFFFFF);

            lbl.PostDrawAndChildUpdate = () => {
                lbl.SetText("ok");
            };

            f1.Children.Add(lbl);

            WindowManager.AddForm(f1);
        }
        #endregion

        private void pictureBox1_Click(object sender, EventArgs e) {

        }

        private void timer1_Tick(object sender, EventArgs e) {
            UpdateWM();
        }

        void UpdateWM() {
            Stopwatch sw = new();
            sw.Start();
            WindowManager.Update();
            sw.Stop();
            Debug.WriteLine("Frametime: " + (sw.Elapsed.TotalMilliseconds - SkiaRenderTarget.overhead).ToString("F2") + "ms (" + sw.Elapsed.TotalMilliseconds.ToString("F2") + "ms including skia overhead)");
            SkiaRenderTarget.overhead = 0;

            // Composition is done, show on picture box
            using (SKImage image = surface.Snapshot())
            using (SKData data = image.Encode())
            using (System.IO.MemoryStream mStream = new System.IO.MemoryStream(data.ToArray())) {
                pictureBox1.Image?.Dispose();
                pictureBox1.Image = new Bitmap(mStream, false);
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            timer1.Enabled = !timer1.Enabled;
        }

        private void button2_Click(object sender, EventArgs e) {
            UpdateWM();
        }

        // stolen from programmingalgorithms.com
        public struct RGB {
            private byte _r;
            private byte _g;
            private byte _b;

            public RGB(byte r, byte g, byte b) {
                this._r = r;
                this._g = g;
                this._b = b;
            }

            public byte R {
                get { return this._r; }
                set { this._r = value; }
            }

            public byte G {
                get { return this._g; }
                set { this._g = value; }
            }

            public byte B {
                get { return this._b; }
                set { this._b = value; }
            }

            public bool Equals(RGB rgb) {
                return (this.R == rgb.R) && (this.G == rgb.G) && (this.B == rgb.B);
            }
        }

        public struct HSL {
            private int _h;
            private float _s;
            private float _l;

            public HSL(int h, float s, float l) {
                this._h = h;
                this._s = s;
                this._l = l;
            }

            public int H {
                get { return this._h; }
                set { this._h = value; }
            }

            public float S {
                get { return this._s; }
                set { this._s = value; }
            }

            public float L {
                get { return this._l; }
                set { this._l = value; }
            }

            public bool Equals(HSL hsl) {
                return (this.H == hsl.H) && (this.S == hsl.S) && (this.L == hsl.L);
            }
        }

        public static RGB HSLToRGB(HSL hsl) {
            byte r = 0;
            byte g = 0;
            byte b = 0;

            if (hsl.S == 0) {
                r = g = b = (byte)(hsl.L * 255);
            } else {
                float v1, v2;
                float hue = (float)hsl.H / 360;

                v2 = (hsl.L < 0.5) ? (hsl.L * (1 + hsl.S)) : ((hsl.L + hsl.S) - (hsl.L * hsl.S));
                v1 = 2 * hsl.L - v2;

                r = (byte)(255 * HueToRGB(v1, v2, hue + (1.0f / 3)));
                g = (byte)(255 * HueToRGB(v1, v2, hue));
                b = (byte)(255 * HueToRGB(v1, v2, hue - (1.0f / 3)));
            }

            return new RGB(r, g, b);
        }

        private static float HueToRGB(float v1, float v2, float vH) {
            if (vH < 0)
                vH += 1;

            if (vH > 1)
                vH -= 1;

            if ((6 * vH) < 1)
                return (v1 + (v2 - v1) * 6 * vH);

            if ((2 * vH) < 1)
                return v2;

            if ((3 * vH) < 2)
                return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);

            return v1;
        }

    }

    public class SkiaRenderTarget : IRenderTarget {
        public static SKCanvas canvas;
        public static double overhead = 0;

        public void DrawBuffer(uint xStart, uint yStart, uint w, uint[] buffer, BufferDrawMode mode) {
            Stopwatch sw = new();
            sw.Start();

            SKBitmap bmp = new();
            var hndl = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            var info = new SKImageInfo((int)w, (int)(buffer.Length / w), SKColorType.Bgra8888, SKAlphaType.Opaque);
            bmp.InstallPixels(info, hndl.AddrOfPinnedObject(), info.RowBytes, null, delegate { hndl.Free(); }, null);

            SKImage img = SKImage.FromBitmap(bmp);
            canvas.DrawImage(img, new SKPoint(xStart, yStart));
            img.Dispose();
            bmp.Dispose();
            /*for(var x = 0; x < w; x++) {
                for(var y = 0; y < buffer.Length / w; y++) {
                    var bV = buffer[(y * w) + x];
                    var c = new SKColor((byte)((bV >> 16) & 0xFF), (byte)((bV >> 8) & 0xFF), (byte)((bV >> 0) & 0xFF));

                    if(c.Red != 0) {
                    hi: // for breakpoint
                        ;
                    }
                    canvas.DrawPoint(new(xStart + x, yStart + y), c);
                }
            }*/

            sw.Stop();
            overhead += sw.Elapsed.TotalMilliseconds;
        }

        public (uint w, uint h) GetSize() {
            return ((uint)Form1.imageInfo.Width, (uint)Form1.imageInfo.Height);
        }
    }
}