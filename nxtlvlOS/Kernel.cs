using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.HAL;
using Cosmos.System;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.Graphics;
using nxtlvlOS.Apps;
using nxtlvlOS.Assets;
using nxtlvlOS.Processing;
using nxtlvlOS.Services;
using nxtlvlOS.Utils;
using nxtlvlOS.Windowing;
using nxtlvlOS.Windowing.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Console = System.Console;
using Sys = Cosmos.System;

namespace nxtlvlOS {
    public class Kernel : Sys.Kernel {
        public static Kernel Instance;

        public NXTLogger Logger = new("Kernel");
        public CosmosVFS VFS = new();
        public spagSVGAII Canvas;

        private int framesRendered = 0;
        private int previousSecond = -1;
        private uint lastAfterHeapCollect = 0;

        private Label fpsLabel = null;

        protected override void BeforeRun() {
            Console.WriteLine(Console.OutputEncoding);
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

            Logger.Log(LogLevel.Info, "Initiliazing SystemPreferenceService");
            ProcessManager.CreateProcess(new SystemPreferenceService(), "SystemPreferenceService");
            Logger.Log(LogLevel.Info, "Initiliazing ContextMenuService"); 
            ProcessManager.CreateProcess(new ContextMenuService(), "ContextMenuService");
            Logger.Log(LogLevel.Info, "Initiliazing UACService");
            ProcessManager.CreateProcess(new UACService(), "UACService");
            Logger.Log(LogLevel.Info, "Initiliazing OOBE");
            ProcessManager.CreateProcess(new OOBE(), "OOBE");
        }

        protected override void Run() {
            TimingUtils.Time("RenderFrame");

            foreach (var proc in ProcessManager.Processes.ToList()) {
                proc.AttachedApp.Update();
            }

            var result = WindowManager.Update();
            
            if(result.Type == WMResultType.Failure) {
                Logger.Log(LogLevel.Fail, result.AdditionalData.ToString());
            }

            Canvas.Display();

            TimingUtils.EndTime("RenderFrame");

            framesRendered++;

            if(framesRendered % 100 == 0) {
                //Logger.Log(LogLevel.Verb, "Since last GC, " + (GCImplementation.GetUsedRAM() - lastAfterHeapCollect)/1024 + " kb were allocated");
                Heap.Collect();
                lastAfterHeapCollect = GCImplementation.GetUsedRAM();
            }

            if(RTC.Second != previousSecond) {
                previousSecond = RTC.Second;

                var perfInfo = "FPS: ca. " + framesRendered + "; Memory: " + Math.Floor((GCImplementation.GetUsedRAM() / 1024f)) + "kb / " + GCImplementation.GetAvailableRAM()*1024 + "kb";
                Logger.Log(LogLevel.Info, perfInfo);
                if (fpsLabel != null) fpsLabel.SetText(perfInfo);

                framesRendered = 0;
            }
        }

        public void Panic(string msg) {
            Logger.Log(LogLevel.Crit, "PANIC! Error: " + msg);
            
            if (Canvas == null) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.Black;

                Console.SetCursorPosition(0, 0);
                Console.WriteLine("Kernel Panic! :(");
                Console.WriteLine("Something went wrong and nxtlvlOS will have to be restarted.");
                Console.WriteLine();
                Console.WriteLine("Error: " + msg);
            } else {
                unchecked {
                    Canvas.Clear((int)0xFFFF0000);
                    Canvas.DrawString("Kernel Panic! :(", Sys.Graphics.Fonts.PCScreenFont.Default, Color.Black, 0, 16);
                    Canvas.DrawString("Something went wrong and nxtlvlOS will have to be restarted.", Sys.Graphics.Fonts.PCScreenFont.Default, Color.Black, 0, 32);
                    Canvas.DrawString("Error: " + msg, Sys.Graphics.Fonts.PCScreenFont.Default, Color.Black, 0, 64);
                    Canvas.Display();
                }
            }

            while (true) ;
        }
    }

    public class SpagSVGAIITarget : IRenderTarget {
        public spagSVGAII Canvas;

        public void DrawBuffer(uint[] buffer) {
            Canvas._xSVGADriver.videoMemory.Copy((uint)Canvas._xSVGADriver.FrameSize, buffer, 0, buffer.Length);
        }

        public (uint w, uint h) GetSize() {
            return ((uint)Canvas.Mode.Width, (uint)Canvas.Mode.Height);
        }
    }
}
