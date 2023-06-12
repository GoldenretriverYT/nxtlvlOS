using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.HAL;
using Cosmos.System;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.Graphics;
using nxtlvlOS.Utils;
using nxtlvlOS.Windowing;
using nxtlvlOS.Windowing.Elements;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Console = System.Console;
using Sys = Cosmos.System;

namespace nxtlvlOS {
    public class Kernel : Sys.Kernel {
        public static Kernel Instance;

        public NXTLogger Logger = new();
        public CosmosVFS VFS = new();
        public spagSVGAII Canvas;

        private int framesRendered = 0;
        private int previousSecond = -1;

        private Label fpsLabel = null;

        protected override void BeforeRun() {
            Instance = this;

            Console.Clear();
            Logger.AddLoggerTarget(new ConsoleLoggerTarget());

            var debuggerTarget = new DebuggerLoggerTarget();
            debuggerTarget.dbg = this.mDebugger;
            Logger.AddLoggerTarget(debuggerTarget);

            Logger.Log(LogLevel.Info, $"Logger initiliazed, min output level: {LogLevelHelpers.GetTag(Logger.MinOutputLevel)}");

            Logger.Log(LogLevel.Info, $"Initiliazing file system");
            VFSManager.RegisterVFS(VFS, false);
            Logger.Log(LogLevel.Info, "Initiliazed file system!");

            Logger.Log(LogLevel.Info, "Initiliazing Window Manager");

            Canvas = new spagSVGAII(new(1280, 720, ColorDepth.ColorDepth32));
            var adapter = new SpagSVGAIITarget();
            adapter.Canvas = Canvas;
            WindowManager.Target = adapter;
            WindowManager.Init();

            SimpleMultiForm();

            Logger.Log(LogLevel.Info, "Initiliazed Window Mananger!");
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

        private void SimpleMultiForm() {
            var f1 = new nxtlvlOS.Windowing.Elements.Form();
            f1.RelativePosX = 0;
            f1.RelativePosY = 0;
            f1.SizeX = 1280;
            f1.SizeY = 720;
            f1.DoNotBringToFront = true;
            f1.SetTitlebarEnabled(false);
            f1.SetTitle("Wow, Form!");

            fpsLabel = new nxtlvlOS.Windowing.Elements.Label();
            fpsLabel.RelativePosX = 5;
            fpsLabel.RelativePosY = 5;
            fpsLabel.SizeX = 100;
            fpsLabel.SizeY = 25;
            fpsLabel.SetText("FPS: ca. ?");

            f1.AddElement(fpsLabel);

            VerticalAlignment[] verticals = new VerticalAlignment[] { VerticalAlignment.Top, VerticalAlignment.Middle, VerticalAlignment.Bottom };

            int vertIdx = 0;
            foreach(var vertical in verticals) {
                var btnOne = new nxtlvlOS.Windowing.Elements.TextButton();
                btnOne.RelativePosX = 50;
                btnOne.RelativePosY = 30 + (70 * vertIdx);
                btnOne.SizeX = 150;
                btnOne.SizeY = 50;
                btnOne.SetHorizontalAlignment(Windowing.Elements.HorizontalAlignment.Left);
                btnOne.SetVerticalAlignment(vertical);
                btnOne.SetText("Button One");

                f1.AddElement(btnOne);

                var btnTwo = new nxtlvlOS.Windowing.Elements.TextButton();
                btnTwo.RelativePosX = 300;
                btnTwo.RelativePosY = 30 + (70 * vertIdx);
                btnTwo.SizeX = 150;
                btnTwo.SizeY = 50;
                btnTwo.SetHorizontalAlignment(Windowing.Elements.HorizontalAlignment.Center);
                btnTwo.SetVerticalAlignment(vertical);
                btnTwo.SetText("Button Two");

                f1.AddElement(btnTwo);

                var btnThree = new nxtlvlOS.Windowing.Elements.TextButton();
                btnThree.RelativePosX = 550;
                btnThree.RelativePosY = 30 + (70 * vertIdx);
                btnThree.SizeX = 150;
                btnThree.SizeY = 50;
                btnThree.SetHorizontalAlignment(Windowing.Elements.HorizontalAlignment.Right);
                btnThree.SetVerticalAlignment(vertical);
                btnThree.SetText("Button Three");

                f1.AddElement(btnThree);

                vertIdx++;
            }

            WindowManager.AddForm(f1);

            for (var x = 0; x < 10; x++) {
                var form = new nxtlvlOS.Windowing.Elements.Form();
                form.RelativePosX = (50 + (x * 50));
                form.RelativePosY = (250 + (x * 25));
                form.SizeX = 200;
                form.SizeY = 200;
                form.SetTitlebarEnabled(true);
                form.SetTitle("Wow, Form! " + x);

                //form.SetBackgroundColor(0x80DEDEDE);
                //form.DrawMode = BufferDrawMode.PixelByPixel;

                if (x == 1) {
                    var toRight = true;
                    form.PreDrawAndChildUpdate = () => {
                        form.RelativePosX += (toRight ? 3 : -3);
                        if (form.RelativePosX > 600) toRight = false;
                        if (form.RelativePosX < 40) toRight = true;

                        form.SetTitle("Wow, " + form.RelativePosX + " " + form.RelativePosY);
                    };
                }

                WindowManager.AddForm(form);
            }
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

        protected override void Run() {
            TimingUtils.Time("RenderFrame");

            var result = WindowManager.Update();
            
            if(result.Type == WMResultType.Failure) {
                Logger.Log(LogLevel.Fail, result.AdditionalData.ToString());
            }

            Canvas.Display();

            TimingUtils.EndTime("RenderFrame");

            framesRendered++;

            if(framesRendered % 10 == 0) {
                Heap.Collect();
            }

            if(RTC.Second != previousSecond) {
                previousSecond = RTC.Second;
                Logger.Log(LogLevel.Info, "FPS: ca. " + framesRendered);

                if (fpsLabel != null) fpsLabel.SetText("FPS: ca. " + framesRendered);
                framesRendered = 0;
            }
        }

        #region temp testing code
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
        #endregion
    }

    public class SpagSVGAIITarget : IRenderTarget {
        public spagSVGAII Canvas;

        public void DrawBuffer(uint xStart, uint yStart, uint w, uint[] buffer, BufferDrawMode mode) {
            Canvas._xSVGADriver.videoMemory.Copy((uint)Canvas._xSVGADriver.FrameSize, buffer, 0, buffer.Length);
        }

        public (uint w, uint h) GetSize() {
            return ((uint)Canvas.Mode.Width, (uint)Canvas.Mode.Height);
        }
    }
}
