#define PURGE_MP

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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
            var menu = new EasyConsole.Menu()
              .Add("Guardar y borrar MPs de la bandeja de entrada", () => GuardarMPs())
              .Add("Salir", () => Environment.Exit(0));

            menu.Display();

            Console.Read();
        }

        private static void GuardarMPs()
        {
            using (IWebDriver web = new ChromeDriver())
            {
                //web.Manage().Window.Maximize();
                web.Navigate().GoToUrl("http://foro.elhacker.net/login.html");

                // Nos logueamos

                web.FindElement(By.Name("user")).SendKeys("");
                web.FindElement(By.Name("passwrd")).SendKeys("");

                web.FindElement(By.CssSelector("input[type='submit']")).Click();

                // Vamos a la bandeja de mensajes

                web.Navigate().GoToUrl("https://foro.elhacker.net/pm.html;f=inbox;sort=date;start=0");

                // Buscamos el numero de páginas

                ReadOnlyCollection<IWebElement> elements = web.FindElements(By.CssSelector("a.navPages"));

                int numberOfPages = int.Parse(elements.Last().Text);

                // Establecemos el encoding de la pagina

                Encoding iso = Encoding.GetEncoding("ISO-8859-1");

                // Recorremos de final a principio

                for (int i = numberOfPages; i >= 1; ++i)
                {
                    // Empezaremos por la última página
                    web.Navigate().GoToUrl($"https://foro.elhacker.net/pm.html;f=inbox;sort=date;start={(i - 1) * 10}");

                    // Obtendremos el source
                    var source = iso.GetBytes(web.PageSource);

                    // Guardaremos el source en el archivo correspondiente
                    File.WriteAllBytes(Path.Combine(DownloadPath, $"Page{i}.html"), source);

#if PURGE_MP
                    // Si esta directiva de preprocesador está activada, los mps de la página iterada serán borrados
                    try
                    {
                        // Se pulsa el checkbox de seleccionar todos los mensajes
                        web.FindElement(By.CssSelector(".titlebg .check")).Click();

                        // Se clica el boton de borrar seleccionados
                        web.FindElement(By.Name("del_selected")).Click();

                        // Y aceptamos el alert de confirmación
                        IAlert alert = web.SwitchTo().Alert();
                        alert.Accept();
                    }
                    catch
                    {
                        // Hemos terminado
                    }
#endif
                }
            }
        }
    }
}