using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace FgNotifier.Models;

public class Notifier
{
    private readonly ChromeDriver _browser;
    private readonly WebDriverWait _wait;
    private readonly List<Point> _cachedMessages = new();
    public readonly List<Game> QueuedGames = new();
    public bool IsLoggedIn;

    public Notifier()
    {
        try
        {
            var service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.EnableVerboseLogging = false;
            var options = new ChromeOptions();
            options.AddArgument("headless");

            _browser = new ChromeDriver(service, options);
            _browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Environments.BROWSER_TIMEOUT_SECONDS);

            _wait = new WebDriverWait(_browser, TimeSpan.FromSeconds(Environments.BROWSER_TIMEOUT_SECONDS));
        }
        catch (Exception e)
        {
            Console.WriteLine($"FATAL Error {e.Message}");
            throw;
        }
    }

    public void SetCookies()
    {
        _browser.Navigate().GoToUrl(Environments.D2JSP_CHAT_URL);

        foreach (var cookie in Environments.D2JSP_COOKIES)
        {
            if (string.IsNullOrEmpty(cookie.Value))
            {
                throw new Exception($"Value for cookie {cookie.Name} is empty");
            }

            _browser.Manage().Cookies.AddCookie(cookie);
        }
    }

    public void GoToChat()
    {
        _browser.Navigate().GoToUrl(Environments.D2JSP_CHAT_URL);
    }

    public void SetTimeout(int timeout)
    {
        _browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(timeout);
    }

    public void CheckIfLoggedIn()
    {
        if (IsLoggedIn)
        {
            return;
        }

        Task.Run(() =>
        {
            // if it fails to find the login message element, that means we are logged in
            try
            {
                _browser.FindElement(By.CssSelector(".ce.p9.B"));
                CheckIfLoggedIn();
            }
            catch
            {
                IsLoggedIn = true;
            }
        });
    }

    public void ClickMessengerButton()
    {
        try
        {
            // wait until the 'd2jsp Instant Messenger' button appears
            var instantMessengerButton = _wait.Until(d => d.FindElement(By.ClassName("chatwu")));

            instantMessengerButton?.Click();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error clicking the 'd2jsp Instant Messenger' button: " + e.Message);
        }
    }

    public List<string> GetNewMessages()
    {
        var allMessages = _wait.Until(d => d.FindElements(By.CssSelector("#iHelpers.Logs > div")));
        var newMessages = allMessages.Where(message => !_cachedMessages.Contains(message.Location)).ToList();

        _cachedMessages.AddRange(newMessages.Select(message => message.Location));

        var textMessages = newMessages.Select(message => message.Text).ToList();
        return textMessages;
    }

    public void SendToD2Bs(Game game)
    {
        Console.WriteLine("Sending to D2BS...");
        File.WriteAllLines(Environments.Settings.GamesFilePath, new[] { game.Username, game.GameName, game.GamePassword, game.FgAmount });
        QueuedGames.Remove(game);
    }

    public void HandleMessage(string message)
    {
        if (message.EndsWith("Connected."))
        {
            Helpers.Log("Connected to chat!, now listening for messages...");
        }

        var match = Environments.FG_RECEIVED_REGEX.Match(message);

        // if a match could not be found, do nothing.
        if (!match.Success)
        {
            return;
        }

        var fgTimestamp = match.Groups[1].Value;
        var fgUsername = match.Groups[2].Value;
        var fgAmount = match.Groups[3].Value;
        var fgMessage = match.Groups[4].Value;

        Helpers.Debug($"{fgAmount}fg received from {fgUsername}");

        if (fgMessage.Split('/').Length < 2)
        {
            Helpers.Debug($"Message from {fgUsername} does not contain a valid game/pass, ignoring...");
            return;
        }

        var messageSplit = fgMessage.Split('/');

        string gameName = messageSplit[0];
        string gamePass = messageSplit[messageSplit.Length - 1];

        Helpers.Log($"Added {fgUsername}'s game to the games queue");
        QueuedGames.Add(new Game
        {
            Username = fgUsername,
            GameName = gameName,
            GamePassword = gamePass,
            FgAmount = fgAmount
        });
    }
}