using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using LogLevel = OpenQA.Selenium.LogLevel;
using Newtonsoft.Json.Linq;

namespace Partslink24Parser
{
    public class Parser
    {
        protected readonly RequestManager _requestManager;
        protected readonly ApplicationContext _context;

        public Parser(RequestManager requestManager, ApplicationContext context)
        {
            _requestManager = requestManager;
            _context = context;
        }

        private bool isElementPresent(ChromeDriver driver, By by)
        {
            try
            {
                driver.FindElement(by);
                return true;
            }
            catch (NoSuchElementException e)
            {
                return false;
            }
        }

        public async Task Run()
        {
            ChromeOptions options = new ChromeOptions();
            //options.AddArguments(new List<string>() { "--headless", "--no-sandbox", "--disable-dev-shm-usage" });
            options.AcceptInsecureCertificates = true;
            options.LeaveBrowserRunning = false;
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.SetLoggingPreference(LogType.Performance, LogLevel.All);

            ChromeDriver driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

            driver.Navigate().GoToUrl("https://www.partslink24.com/partslink24/user/login.do");

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            driver.FindElement(By.CssSelector("div#usercentrics-root")).GetShadowRoot().FindElement(By.CssSelector("button[data-testid=\"uc-accept-all-button\"]")).Click();

            //wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[.='Accept All']"))).Click();

            driver.FindElement(By.Id("login-id")).SendKeys("ua-915030");
            driver.FindElement(By.Id("login-name")).SendKeys("admin");
            driver.FindElement(By.Id("inputPassword")).SendKeys("Idonot002");
            driver.FindElement(By.Id("login-btn")).Click();

            if (isElementPresent(driver, By.CssSelector("a#squeezeout-login-btn")))
            {
                driver.FindElement(By.CssSelector("a#squeezeout-login-btn")).Click();
            }

            driver.FindElement(By.CssSelector("input[placeholder=\"Search VIN\"]")).SendKeys("WVWA12608CT025946");
            //wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[placeholder=\"Search VIN\"]"))).SendKeys("WVWA12608CT025946");

            driver.FindElement(By.CssSelector("div[class=\"search-txt\"]")).Click();

            Dictionary<string, string>  _headers = new Dictionary<string, string>();

            var logs = driver.Manage().Logs;

            var perf = logs.GetLog(LogType.Performance);

            var item = perf.Select(x => x.Message).Where(x => x.Contains("referer") && x.Contains("authority") && x.Contains("cookie") && x.Contains("authorization")).First();

            JObject result = JObject.Parse(item);

            var headers = result["message"]["params"]["headers"];

            foreach (JProperty prop in headers.OfType<JProperty>())
            {
                if (prop.Name != ":method" || prop.Name != ":path" || prop.Name != ":scheme" || prop.Name != "accept-encoding")
                {
                    if(prop.Name != ":authority")
                        _headers.Add("authority", prop.Value.ToString());
                    else
                        _headers.Add(prop.Name, prop.Value.ToString());
                }
            }

            Console.WriteLine(_headers);

            driver.Dispose();

        }
    }
}
