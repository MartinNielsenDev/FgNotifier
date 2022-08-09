using System;
using System.IO;
using System.Reflection;
using FgNotifier.Models;
using Newtonsoft.Json;

namespace FgNotifier;

public class Helpers
{
    public static void Log(string message)
    {
        var timestamp = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
        var output = $"[{timestamp}] {message}";
        Console.WriteLine(output);
        if(Environments.Settings?.LogFilePath != null)
        {
            File.AppendAllText(Environments.Settings.LogFilePath, output + Environment.NewLine);
        }
    }

    public static void Debug(string message)
    {
        if (Environments.Settings.DebugLogging)
        {
            Log(message);
        }
    }

    public static void SaveSettings(Settings settings)
    {
        try
        {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText("settings.json", json);
        }
        catch
        {
            // ignored
        }
    }
}