using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Godot;
using Environment = System.Environment;

namespace Game.Utils;

public static class Log
{
    public enum Level
    {
        Debug,
        Info,
        Warn,
        Error
    }

    public static Level LogLevel = OS.IsDebugBuild() ? Level.Debug : Level.Info;

    [Conditional("DEBUG")]
    public static void Debug(
        object msg = null,
        [CallerFilePath] string filePath = null,
        [CallerMemberName] string memberName = null
    ) => Print(Level.Debug, Format(filePath, memberName, $"[Debug] {msg}"));

    public static void Info(
        object msg = null,
        [CallerFilePath] string filePath = null,
        [CallerMemberName] string memberName = null
    ) => Print(Level.Info, Format(filePath, memberName, $"[Info] {msg}"));

    public static void Warn(
        object msg = null,
        [CallerFilePath] string filePath = null,
        [CallerMemberName] string memberName = null
    ) => Print(Level.Warn, Format(filePath, memberName, $"[Warn] {msg}"));

    public static void Error(
        Exception e,
        [CallerFilePath] string filePath = null,
        [CallerMemberName] string memberName = null
    ) => Print(Level.Error, Format(filePath, memberName, e));

    private static string Format(
        string filePath,
        string memberName,
        object msg
    ) => msg is null
        ? Environment.NewLine
        : $"{Timestamp()}{Runtime()}{FileName(filePath)}{(memberName is not null ? $"::{MemberName(memberName)}" : "]")}{msg}{Environment.NewLine}";

    private static string Timestamp() => DateTime.Now.ToString("[HH:mm:ss.fff]");
    private static string Runtime() => $"[{Stopwatch.Elapsed.Format()}]";
    private static string FileName(string x) => $"[{Path.GetFileNameWithoutExtension(x)}";
    private static string MemberName(string x) => x is not null ? $"{x}]" : null;

    private static string Format(this TimeSpan value, string noTimeStr = "0ms")
    {
        var timeStr = value.ToString("d'.'hh':'mm':'ss'.'fff'ms'").TrimStart('0', ':', '.');
        return timeStr == "ms" ? noTimeStr
            : !timeStr.Contains('.') ? $".{timeStr}"
            : timeStr;
    }

    private static readonly string LogFile;
    private static readonly Stopwatch Stopwatch;

    static Log()
    {
        Stopwatch = Stopwatch.StartNew();

        var dir = ProjectSettings.GlobalizePath(OS.IsDebugBuild() ? "res://data/logs" : "user://data/logs");
        LogFile = $"{dir}/{DateTime.Now:yyyy-MM-dd}.log";

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir!);

        if (!File.Exists(LogFile))
            File.Create(LogFile).Close();

        Debug($"*** NEW COMPILATION DETECTED: {DateTime.Now:HH:mm:ss.fff} ***");
    }

    private static void Print(Level level, string msg)
    {
        if (level < LogLevel) return;

        lock (LogFile)
        {
            try
            {
                Console.Write(msg);
                File.AppendAllText(LogFile, $"{msg}");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to write to log file: {e.Message}");
            }
        }
    }
}