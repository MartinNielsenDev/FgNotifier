using System.Text.RegularExpressions;
using FgNotifier.Models;
using OpenQA.Selenium;

namespace FgNotifier;

public class Environments
{
    public static readonly string D2JSP_CHAT_URL = "https://forums.d2jsp.org/chat.php";
    public static readonly int BROWSER_TIMEOUT_SECONDS = 8;
    public static readonly Regex FG_RECEIVED_REGEX = new(@"(\d+:\d+:\d+)\s(.+)\shas\ssent\syou\s(\d+)\sForum\sGold\swith\sthe\scomment:\s(.+)");
    public static Cookie[] D2JSP_COOKIES;
    
    public static Settings Settings;
}