using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Interactions;
using System.Diagnostics;
using System.IO;
using System.Data;
using System.Threading;
using System.Windows;
using RedbubbleInput;
using System.Windows.Forms;
using Keys = OpenQA.Selenium.Keys;

namespace Redbubble
{
    public class Processor
    {
        #region Properties 
        public ChromeDriver driver { get; set; }
        public Random random { get; set; } = new Random();
        public int TimeOffset { get; set; } = 1;
        static Processor __Instance;
        public static string DebugOutFile = "LogError.txt";
        WebDriverWait wait;
        IJavaScriptExecutor javaScriptExecutor;
        #endregion
        #region Singleton
        public static Processor GetInstance()
        {
            try
            {
                if (__Instance == null)
                    __Instance = new Processor();
            }
            catch { }
            return __Instance;
        }
        #endregion
        #region Dispose
        public void Dispose()
        {
            if (driver != null)
            {
                try
                {
                    driver.Dispose();
                }
                catch { }
            }
        }
        #endregion
        #region Debug
        static void DebugOut(Exception e, string header)
        {
            Debug.WriteLine(header);
            Debug.WriteLine(e);
            File.AppendAllLines(DebugOutFile, new List<string> { DateTime.Now.ToString() + ":" + header, e.ToString() });
        }
        #endregion
        #region Key sender 
        private void SendKey(By elementBy, string key, bool wordbyword = false, bool eraseText = false, int loopCount = 1)
        {
            RandomSleep(2009, 3090);
            wait.Until(ExpectedConditions.ElementExists(elementBy));
            IWebElement webElement = driver.FindElement(elementBy);
            RandomScroll(webElement);
            if (eraseText)
            {
                javaScriptExecutor.ExecuteScript("arguments[0].value = \"\"", webElement);
            }
            for (int i = 0; i < loopCount; i++)
            {
                if (wordbyword)
                    for (int j = 0; j < key.Length; j++)
                    {
                        webElement.SendKeys(key[j].ToString());
                        RandomSleep(10, 50);
                    }
                else
                    webElement.SendKeys(key);
                RandomSleep(100, 333);
            }
            RandomSleep(1000, 2000);
        }
        private void SendKey(string key, bool wordbyword = false, int loopCount = 1)
        {
            for (int i = 0; i < loopCount; i++)
            {
                Actions actions = new Actions(driver);
                if (wordbyword)
                    for (int j = 0; j < key.Length; j++)
                    {
                        Actions subActions = new Actions(driver);
                        subActions.SendKeys(key[j].ToString());
                        subActions.Perform();
                        RandomSleep(10, 50);
                    }
                else
                {
                    actions.SendKeys(key);
                    actions.Perform();
                }
                RandomSleep(100, 333);
            }
        }
        #endregion
        #region Slider Handler
        void HandleSlider(By sliderBy, By valueBy, int expectedValue, Func<string, string> processValue, IWebElement parent = null)
        {
            wait.Until(ExpectedConditions.ElementExists(sliderBy));
            wait.Until(ExpectedConditions.ElementExists(valueBy));
            Click(sliderBy, true, parent);
            IWebElement valueElement = parent == null ? driver.FindElement(valueBy) : parent.FindElement(valueBy);
            while (true)
            {
                int sliderValue = int.Parse(processValue(valueElement.GetAttribute("innerText")));
                if (sliderValue == expectedValue)
                    break;
                else if (sliderValue < expectedValue)
                    SendKey(Keys.Right, false, expectedValue - sliderValue);
                else
                    SendKey(Keys.Left, false, sliderValue - expectedValue);
            }
        }
        #endregion
        #region Random 
        public void RandomSleep(int a, int b)
        {
            Thread.Sleep(random.Next((int)(a * TimeOffset), (int)(b * TimeOffset)));
        }
        public void RandomScroll(IWebElement webElement)
        {
            int offset = random.Next();
            javaScriptExecutor.ExecuteScript($"window.scrollTo({webElement.Location.X}, {webElement.Location.Y - offset % 500 - 50 });");
        }
        #endregion
        #region Real Click
        void Click(By elementBy, bool use_element_click = false, IWebElement parentElement = null)
        {
            wait.Until(ExpectedConditions.ElementExists(elementBy));
            IWebElement element = null;
            if (parentElement == null)
                element = driver.FindElement(elementBy);
            else
            {
                element = parentElement.FindElement(elementBy);
            }
            RandomScroll(element);
            RandomSleep(1230, 2090);
            Actions actions = new Actions(driver);
            if (!use_element_click)
                actions.MoveToElement(element, 3, 2).Click().Build().Perform();
            else
                element.Click();
            RandomSleep(1230, 2090);
        }
        #endregion
        #region Test bot detection
        void Test()
        {
            //Test
            driver.Navigate().GoToUrl("https://antoinevastel.com/bots/");
            driver.Navigate().GoToUrl("https://bot.sannysoft.com/");
            driver.Navigate().GoToUrl("https://arh.antoinevastel.com/bots/areyouheadless");
        }
        #endregion
        #region Contructor
        Processor()
        {

        }
        #endregion
        #region Create driver
        public void CreateDriver()
        {
            Dispose();
            ChromeOptions opt = new ChromeOptions();
            ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService();
            opt.DebuggerAddress = "127.0.0.1:9222";
            chromeDriverService.HideCommandPromptWindow = true;
            driver = new ChromeDriver(chromeDriverService, opt);

            wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(30000));

            javaScriptExecutor = (IJavaScriptExecutor)driver;
        }
        #endregion
        #region Go to Add New Work
        public void GoToAddNewWork(int method)
        {
            try
            {
                driver.Manage().Window.Maximize();
            }
            catch
            {

            }
            driver.Navigate().GoToUrl("https://www.redbubble.com/explore/for-you/");
            Thread.Sleep(2000);
            Click(By.CssSelector("button div[data-testid='ds-avatar']"), false);
            //javaScriptExecutor.ExecuteScript("document.querySelector(\"button div[data-testid='ds-avatar']\").click()"); 
            if (method == 1)
            {
                try
                {
                    Click(By.CssSelector("a[href='/portfolio/images/new?ref=account-nav-dropdown']"), true);
                    
                }
                catch
                {
                    Click(By.CssSelector("a[href='/studio/dashboard']"), true);
                    Click(By.CssSelector("a[href='/portfolio/images/new?ref=dashboard']"), true);
                }
               
            }
            else if (method == 2)
            {
                Click(By.CssSelector("a[title*='Manage Portfolio']"), true);
                Click(By.CssSelector("div[class*='manage-works-nav_heading-actions'] > a[href*='/portfolio/images/new']"), true);
            }
        }
        #endregion
        #region Go to copy region 
        public void GoToCopyRegion(int method)
        {
            GoToAddNewWork(method);

            Click(By.CssSelector("i[class='copy-icon']"), true);
            Click(By.CssSelector("div[class*='works_work-menu-link']"), true);
            Click(By.CssSelector("a[class*='works_work-menu-option__duplicate']"), true);
        }
        #endregion
        #region Provide Information
        void AddInfo(RedbubbleInputInfo info)
        {
            SendKey(By.CssSelector("#work_title_en"), info.Title, true, true);
            SendKey(By.CssSelector("#work_tag_field_en"), info.Tags, true, true);
            SendKey(By.CssSelector("#work_description_en"), info.Description, true, true);
            Click(By.CssSelector("div[class*='global-background-color-setting'] > div[class*='sp-replacer']"), true);
            SendKey(By.CssSelector("div[class='sp-container sp-light sp-buttons-disabled sp-palette-disabled']  .sp-input"), Keys.Backspace, false, false, 7);
            SendKey(By.CssSelector("div[class='sp-container sp-light sp-buttons-disabled sp-palette-disabled']  .sp-input"), info.BackgroundColor, true);
            SendKey(By.CssSelector("div[class='sp-container sp-light sp-buttons-disabled sp-palette-disabled']  .sp-input"), Keys.Enter);
        }
        #endregion
        #region Scale All Products
        public void ScaleAllProducts()
        {
            List<string> scaleConfigs = File.ReadAllLines("scaleconfig.txt").ToList();
            wait.Until(ExpectedConditions.ElementExists(By.CssSelector(".product-row > div")));
            var products = driver.FindElementsByCssSelector(".product-row > div");
            for (int i = 0; i < products.Count; i++)
            {
                try
                {
                    var product = products[i];

                    //Click enable
                    if (product.GetAttribute("class").ToLower().Contains("all-disabled"))
                        Click(By.CssSelector(".enable-all"), true, product);
                    //Click edit button
                    Click(By.CssSelector(".edit-product"), true, product);
                    //Get expanded edit panel
                    wait.Until(ExpectedConditions.ElementExists(By.CssSelector(".expanded  ")));
                    var editPanel = driver.FindElement(By.CssSelector(".expanded "));
                    By sliderValueBy = By.CssSelector(".design-size-value");
                    By sliderBy = By.CssSelector(".design-size");
                    try
                    {
                        editPanel.FindElement(sliderValueBy);
                    }
                    catch
                    {
                        sliderValueBy = By.CssSelector("span[class*='scaleValue']");
                        editPanel.FindElement(sliderValueBy);
                        sliderBy = By.CssSelector("input[name*='design-size']");
                    }
                    //Slide the image scale slider
                    HandleSlider(sliderBy, sliderValueBy, int.Parse(scaleConfigs[i]) - random.Next() % 3, a => a.Replace('%', ' '), editPanel);
                    Click(By.CssSelector(".vertical"), true, editPanel);
                    Click(By.CssSelector(".horizontal"), true, editPanel);
                    Click(By.CssSelector("button[class*='apply-changes']"), true, editPanel);
                }
                catch
                {

                }
            }
        }
        #endregion
        #region Upload  By Copy
        public void UploadByCopy(RedbubbleInputInfo info)
        {
            //Apply copy method
            GoToCopyRegion(info.UploadMethod);

            //Upload image
            SendKey(By.Id("select-image-base"), info.Image);
            RandomSleep(2040, 3045);

            //Add required info
            AddInfo(info);
        }
        #endregion
        #region Upload By Image
        public void UploadByImage(RedbubbleInputInfo info)
        {
            GoToAddNewWork(/*random.Next() % 2 +*/ 1);

            //Upload image
            SendKey(By.CssSelector("#select-image-single"), info.Image, false);

            //Add required info
            AddInfo(info);

            //Scale all products 
            ScaleAllProducts();


        }
        #endregion
        #region Entry
        public bool Begin(RedbubbleInputInfo info)
        {
            try
            {
                //try
                //{
                //    wait.Timeout = TimeSpan.FromMilliseconds(10000);
                //    Click(By.CssSelector("a[title*='Redbubble']"), true);
                //}
                //catch {
                //    Click(By.CssSelector("img[class*='node_modules--redbubble-design-system-react-headerAndFooter-components-Footer-FooterLegal-themes-default__redbubbleLogo--']"), true);
                //}
                wait.Timeout = TimeSpan.FromMilliseconds(30000);
                if (info.UploadMethod <= 2)
                {
                    UploadByCopy(info);
                }
                else if (info.UploadMethod == 3)
                {
                    UploadByImage(info);
                    Click(By.CssSelector("#media_painting"), true);
                    Click(By.CssSelector("#media_digital"), true);
                }

                else
                    return false;
                //Demo 
                Click(By.CssSelector("#work_hidden_false"), true);
                Click(By.CssSelector("#work_safe_for_work_true"), true);
                Click(By.CssSelector("#rightsDeclaration"), true);
                Click(By.CssSelector("#submit-work"), true);
                try
                {
                    wait.Until(ExpectedConditions.UrlContains("promote"));
                }
                catch
                {
                    wait.Until(ExpectedConditions.UrlContains("works"));
                }
            }
            catch (Exception e)
            {
                DebugOut(e, "Upload image");
                return false;
            }
            return true;
        }
        #endregion
    }
}
