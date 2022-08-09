using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using FgNotifier.Models;
using Newtonsoft.Json;
using OpenQA.Selenium;

namespace FgNotifier
{
    internal class Program
    {
        public static void Main()
        {
            // initialize settings
            Initialize();
            
            // initialize the notifier
            var notifier = new Notifier();
            notifier.SetCookies();

            // infinite loop
            while (true)
            {
                Helpers.Log("Setting up the notifier, please wait...");
                // set timeout to default value set in Constants 
                notifier.SetTimeout(Environments.BROWSER_TIMEOUT_SECONDS);
                // navigate to the d2jsp chat
                notifier.GoToChat();
                // set timeout to 0 seconds so we can check if we're logged in quickly
                notifier.SetTimeout(0);
                // check if logged in
                notifier.CheckIfLoggedIn();
    
                // wait to be logged in, checking 20 times every 0.5 second (10 seconds in total)
                int retries;
                for (retries = 0; retries < 20; retries++)
                {
                    if (notifier.IsLoggedIn)
                    {
                        Helpers.Log("Successfully logged into d2jsp, waiting for forum gold...");
                        notifier.ClickMessengerButton();
                        break;
                    }

                    Helpers.Debug($"Not logged in, retrying... ({retries+1}/20)");
                    Thread.Sleep(500);
                }

                // start the message read loop if we successfully logged in, otherwise restart the loop
                while (notifier.IsLoggedIn)
                {
                    var messages = notifier.GetNewMessages();

                    foreach (var message in messages)
                    {
                        notifier.HandleMessage(message);
                    }

                    foreach (var gameToJoin in notifier.QueuedGames)
                    {
                        notifier.SendToD2Bs(gameToJoin);
                    }

                    Thread.Sleep(500);
                }

                Helpers.Log("Failed to log into d2jsp, make sure your cookies are correctly set, trying again in 5 seconds...");
                Thread.Sleep(5000);
            }
        }

        private static void Initialize()
        {
            try
            {
                Helpers.Log("Loading settings...");

                if (File.Exists("settings.json"))
                {
                    var content = File.ReadAllText("settings.json");
                    var json = JsonConvert.DeserializeObject<Settings>(content);
                    Environments.Settings = json;
                    Helpers.SaveSettings(json);
                }
                else
                {
                    Helpers.SaveSettings(new Settings());
                }
            }
            catch
            {
                Helpers.Log("Error reading settings.json, could be malformed JSON.\r\n\r\nTry to delete settings.json file and restart the program.");
            }
            
            
            if (Environments.Settings is null)
            {
                Helpers.Log("This appears to be your first time running the program.\r\n\r\nPlease fill out the settings.json file and restart the program.");
                Console.ReadKey();
                Environment.Exit(0);
            }
            
            
            Environments.D2JSP_COOKIES = new Cookie[]
            {
                new("flags", "8"),
                new("member_id", Environments.Settings.MemberId),
                new("msec", Environments.Settings.MSec)
            };
        }
    }
}