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
        public LogLevel MinOutputLevel { get; set; }
        private readonly List<LoggerTarget> loggerTargets = new();

        public NXTLogger(LogLevel minOutputLevel = LogLevel.Info) {
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
                target.Write(tag, colors.Item1, colors.Item2);
                target.Write(msg + "\n", Color.White, ConsoleColor.White);
            }
        }
    }

    public enum LogLevel {
        Crit,
        Fail,
        Warn,
        Info,
        Verb,
        Sill,
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

        public override void Write(string msg, Color colorNorm, ConsoleColor consoleColor) {
            dbg.Send(msg);
        }
    }
}
