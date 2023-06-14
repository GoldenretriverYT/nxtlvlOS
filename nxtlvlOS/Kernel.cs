using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.HAL;
using Cosmos.System;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.Graphics;
using nxtlvlOS.Apps;
using nxtlvlOS.Processing;
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

            Logger.Log(LogLevel.Info, "Initiliazed Window Mananger!");

            ProcessManager.CreateProcess(new UACService(), "UACService");
            ProcessManager.CreateProcess(new OOBE(), "OOBE");
        }

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

                var perfInfo = "FPS: ca. " + framesRendered + "; Memory: " + Math.Floor((GCImplementation.GetUsedRAM() / 1024f / 1024f)) + "mb / " + GCImplementation.GetAvailableRAM() + "mb";
                Logger.Log(LogLevel.Info, perfInfo);
                if (fpsLabel != null) fpsLabel.SetText(perfInfo);

                framesRendered = 0;
            }
        }
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
