using Cosmos.Core.Memory;
using Cosmos.Debug.Kernel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS {
    public class NXTLogger {
        public string LoggerName { get; set; }
        public LogLevel MinOutputLevel { get; set; }
        private readonly List<LoggerTarget> loggerTargets = new();

        public NXTLogger(string loggerName, LogLevel minOutputLevel = LogLevel.Info) {
            this.LoggerName = loggerName;
            this.MinOutputLevel = minOutputLevel;
        }

        public void AddLoggerTarget(LoggerTarget target) {
            loggerTargets.Add(target);
        }

        public void RemoveLoggerTarget(LoggerTarget target) {
            loggerTargets.Remove(target);
        }

        public void Log(LogLevel level, string msg) {
            var colors = LogLevelHelpers.GetLevelColor(level);
            var tag = LogLevelHelpers.GetTag(level);

            foreach(var target in loggerTargets) {
                target.Write("[" + LoggerName + "] ", Color.White, ConsoleColor.White);
                target.Write(tag, colors.Item1, colors.Item2);
                target.Write(msg + "\n", Color.White, ConsoleColor.White);
            }
        }

        public static NXTLogger Create(string name, bool debuggerLog = true) {
            var logger = new NXTLogger(name, LogLevel.Info);

            if (debuggerLog) {
                var target = new DebuggerLoggerTarget();
                target.dbg = Kernel.Instance.mDebugger;

                logger.AddLoggerTarget(target);
            }
            
            return logger;
        }
    }

    public enum LogLevel {
        Crit = 0,
        Fail = 1,
        Warn = 2,
        Info = 3,
        Verb = 4,
        Sill = 5,

        // enums can apparently have aliases! didnt know that
        Error = Fail,
    }

    public static class LogLevelHelpers {
        public static (Color, ConsoleColor) GetLevelColor(this LogLevel level) =>
            level switch {
                LogLevel.Crit => (Color.DarkRed, ConsoleColor.DarkRed),
                LogLevel.Fail => (Color.Red, ConsoleColor.Red),
                LogLevel.Warn => (Color.Yellow, ConsoleColor.Yellow),
                LogLevel.Info => (Color.LightBlue, ConsoleColor.Blue),
                LogLevel.Verb => (Color.Purple, ConsoleColor.Magenta),
                LogLevel.Sill => (Color.White, ConsoleColor.White),
                _             => (Color.White, ConsoleColor.White)
            };

        public static string GetTag(this LogLevel level) =>
                    level switch {
                        LogLevel.Crit => "[CRIT] ",
                        LogLevel.Fail => "[FAIL] ",
                        LogLevel.Warn => "[WARN] ",
                        LogLevel.Info => "[Info] ",
                        LogLevel.Verb => "[Verb] ",
                        LogLevel.Sill => "[Sill] ",
                        _             => "[????] "
                    };


    }

    public abstract class LoggerTarget {
        public abstract void Write(string msg, Color colorNorm, ConsoleColor consoleColor);
    }

    public class ConsoleLoggerTarget : LoggerTarget {
        public override void Write(string msg, Color colorNorm, ConsoleColor consoleColor) {
            Console.ForegroundColor = consoleColor;
            Console.Write(msg);
        }
    }

    public class DebuggerLoggerTarget : LoggerTarget {
        public Debugger dbg;
        string letHimCook = "";

        public override void Write(string msg, Color colorNorm, ConsoleColor consoleColor) {
            letHimCook += msg;

            if(msg.Contains("\n")) {
                dbg.Send(letHimCook.Substring(0, letHimCook.Length - 1));
                letHimCook = "";
            }
        }
    }
}
