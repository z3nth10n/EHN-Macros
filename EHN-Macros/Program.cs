#define PURGE

using EHN_Macros.Properties;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace EHN_Macros
{
    internal class Program
    {
        private static string DownloadPath
        {
            get
            {
                string folder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Downloads");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                return folder;
            }
        }

        private static void Main(string[] args)
        {
            //var options = new ChromeOptions();
            //options.AddUserProfilePreference("download.default_directory", DownloadPath);

            using (IWebDriver web = new ChromeDriver())
            {
                //web.Manage().Window.Maximize();
                web.Navigate().GoToUrl("http://foro.elhacker.net/login.html");

                web.FindElement(By.Name("user")).SendKeys(Resources.User);
                web.FindElement(By.Name("passwrd")).SendKeys(Resources.Password);

                web.FindElement(By.CssSelector("input[type='submit']")).Click();

                web.Navigate().GoToUrl("https://foro.elhacker.net/pm.html;f=inbox;sort=date;start=0");

                ReadOnlyCollection<IWebElement> elements = web.FindElements(By.CssSelector("a.navPages"));

                int numberOfPages = int.Parse(elements.Last().Text);

                Encoding iso = Encoding.GetEncoding("ISO-8859-1");

                for (int i = numberOfPages; i >= 1; ++i)
                {
                    web.Navigate().GoToUrl($"https://foro.elhacker.net/pm.html;f=inbox;sort=date;start={(i - 1) * 10}");

                    var source = iso.GetBytes(web.PageSource);

                    File.WriteAllBytes(Path.Combine(DownloadPath, $"Page{i}.html"), source);

#if PURGE
                    try
                    {
                        web.FindElement(By.CssSelector(".titlebg .check")).Click();
                        web.FindElement(By.Name("del_selected")).Click();

                        // Aceptamos el alert
                        IAlert alert = web.SwitchTo().Alert();
                        alert.Accept();
                    }
                    catch
                    {
                        // Hemos terminado
                    }

                    //Thread.Sleep(1500);
#endif
                }
            }

            Console.Read();
        }
    }
}