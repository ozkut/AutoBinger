using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using AutoBinger.Properties;
using OpenQA.Selenium.DevTools.V132.WebAuthn;

namespace AutoBinger
{
    class Program
    {
        private static void Exit(string msg = "", bool exit = false)
        {
            Console.WriteLine(msg + "\nPress any key to exit...");
            Console.ReadKey();
            if (exit)
                Environment.Exit(Environment.ExitCode);
        }

        public static async Task Main(string[] args)
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            string currentFolder = Path.Combine([Environment.CurrentDirectory, "selenium-manager", (isWindows ? "windows" : "linux")]);
            _ = Directory.CreateDirectory(currentFolder);
            await File.WriteAllBytesAsync(Path.Combine(currentFolder, isWindows ? "selenium-manager.exe" : "selenium-manager"), isWindows ? Resources.selenium_manager_win : Resources.selenium_manager_linux);

            string filePath = Path.Combine(Environment.GetFolderPath(isWindows ? Environment.SpecialFolder.Desktop : Environment.SpecialFolder.UserProfile), "terms.txt");
            if (!File.Exists(filePath))
                Exit($"The file \"{filePath}\" does not exits!", true);

            string[] terms = await File.ReadAllLinesAsync(filePath);
            if (!int.TryParse(terms[0], out int delay) && delay >= 2)
                Exit($"Invalid delay on first line of \"{filePath}\"!", true);

            bool isValidUnattendedVar = bool.TryParse(terms[1], out bool isUnattended);
            if (!isValidUnattendedVar)
                isUnattended = true;
            
            using EdgeDriver driver = new();
            driver.Manage().Window.Maximize();
            await driver.Navigate().GoToUrlAsync("https://www.bing.com");
            await Task.Delay(delay);
            
            IWebElement cookieButton = driver.FindElement(By.Id("bnp_btn_reject"));
            cookieButton.Click();
            await Task.Delay(delay / 2);

            for (int i = 1 + (!isValidUnattendedVar ? 0 : 1); i < terms.Length; i++)
            {
                IWebElement searchBox = driver.FindElement(By.Name("q"));
                searchBox.Clear();
                searchBox.SendKeys(terms[i]);
                searchBox.SendKeys(Keys.Enter);
                await Task.Delay(delay);
            }

            if (!isUnattended)
                Exit();
            driver.Quit();
        }
    }
}