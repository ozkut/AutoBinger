using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace AutoBinger
{
    class Program
    {
        private static void WaitForKeyPress(string msg, bool exit = false)
        {
            Console.WriteLine(msg + "\nWaiting for key press...");
            Console.ReadKey();
            if (exit)
                Environment.Exit(Environment.ExitCode);
        }

        public static async Task Main(string[] args)
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            string managerFile = Path.Combine(Directory.CreateDirectory(Path.Combine([Environment.CurrentDirectory, "selenium-manager", (isWindows ? "windows" : "linux")])).FullName, $"selenium-manager{(isWindows ? ".exe" : string.Empty)}");
            if (!File.Exists(managerFile))
                await File.WriteAllBytesAsync(managerFile, isWindows ? Properties.Resources.selenium_manager_win : Properties.Resources.selenium_manager_linux);

            string filePath = Path.Combine(Environment.GetFolderPath(isWindows ? Environment.SpecialFolder.Desktop : Environment.SpecialFolder.UserProfile), "terms.txt");
            if (!File.Exists(filePath))
                WaitForKeyPress($"The file \"{filePath}\" does not exits!", true);

            string[] terms = await File.ReadAllLinesAsync(filePath);
            if (!int.TryParse(terms[0], out int delay) && delay >= 2)
                WaitForKeyPress($"Invalid delay on first line of \"{filePath}\"!", true);

            bool isValidUnattendedVar = bool.TryParse(terms[1], out bool isUnattended);
            if (!isValidUnattendedVar)
                isUnattended = true;
            
            using EdgeDriver driver = new();
            driver.Manage().Window.Maximize();
            await driver.Navigate().GoToUrlAsync("https://www.bing.com");
            await Task.Delay(delay);
            
            IWebElement cookieButton = driver.FindElement(By.Id("bnp_btn_reject"));
            cookieButton.Click();

            if (!isUnattended)
            {
                WaitForKeyPress("Ready!");
                Console.WriteLine("Start");
            }

            for (int i = 1 + (!isValidUnattendedVar ? 0 : 1); i < terms.Length; i++)
            {
                IWebElement searchBox = driver.FindElement(By.Name("q"));
                searchBox.Clear();
                searchBox.SendKeys(terms[i]);
                searchBox.SendKeys(Keys.Enter);
                await Task.Delay(delay);
            }

            if (!isUnattended)
                WaitForKeyPress("Finished");
            driver.Quit();
        }
    }
}