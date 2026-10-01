using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class Logging
{
    private static LoggingSettings _settings;

    public static void Initialize(LoggingSettings settings)
    {
        _settings = settings;
    }

    public static void Log(
        string message,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        if (_settings == null || !_settings.DisplayLogging)
            return;

        Debug.Log($"[{Path.GetFileNameWithoutExtension(callerFilePath)}.{callerName}:{callerLineNumber}] {message}");
    }
}
