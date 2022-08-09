namespace FgNotifier.Models;

public class Settings
{
    public bool DebugLogging { get; set; } = false;
    public string GamesFilePath { get; set; } = @"C:\d2bs\fgnotifier.game";
    public string LogFilePath { get; set; } = "fgnotifier.log";
    public string MemberId { get; set; } = "";
    public string MSec { get; set; } = "";
}