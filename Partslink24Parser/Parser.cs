using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using LogLevel = OpenQA.Selenium.LogLevel;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using System.Security.Policy;
using System.Xml.Linq;
using Partslink24Parser.Entities;
using System;

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

        private string GetString(ChromeDriver driver, string id)
        {
            return driver.FindElement(By.Id(id)).FindElement(By.CssSelector("div[class=\"p5_table_cell_comp p5t13_value\"]")).Text;
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

            driver.FindElement(By.CssSelector("div[class=\"search-txt\"]")).Click();

            Dictionary<string, string> _headers = new Dictionary<string, string>();

            driver.FindElement(By.CssSelector(".p5_vehicle_info_vin"));

            var logs = driver.Manage().Logs;

            var perf = logs.GetLog(LogType.Performance);

            var item = perf.Select(x => x.Message).Where(x => x != null && x.Contains("/1.1/authorize") && x.Contains("referer") && x.Contains("cookie") && x.Contains("authority") && x.Contains("x-requested-with")).FirstOrDefault();

            JObject result = JObject.Parse(item);

            var headers = result["message"]["params"]["headers"];

            foreach (JProperty prop in headers.OfType<JProperty>())
            {
                if (prop.Name != "content-length" && prop.Name != "content-type" && prop.Name != ":method" && prop.Name != ":path" && prop.Name != ":scheme" && prop.Name != "accept-encoding")
                {
                    if (prop.Name == ":authority")
                        _headers.Add("authority", prop.Value.ToString());
                    else
                        _headers.Add(prop.Name, prop.Value.ToString());
                }
            }

            string cookie = $"_gcl_au={driver.Manage().Cookies.GetCookieNamed("_gcl_au").Value}; _fbp={driver.Manage().Cookies.GetCookieNamed("_fbp").Value}; PL24SESSIONID={driver.Manage().Cookies.GetCookieNamed("PL24SESSIONID").Value}; pl24LoggedInTrail={driver.Manage().Cookies.GetCookieNamed("pl24LoggedInTrail").Value}; PL24TOKEN={driver.Manage().Cookies.GetCookieNamed("PL24TOKEN").Value}";

            _headers["cookie"] = cookie;

            _requestManager.AddHeaders(_headers);

            driver.Dispose();

            var token = await _requestManager.Post("https://www.partslink24.com/auth/ext/api/1.1/authorize",
                new { serviceNames = new string[] { "cart", "config_parts", "pl24-full-vin-data", "pl24-sendbtmail", "pl24-orderbridge", "vw_parts" }, withLogin = true});

            VehicleData vehicleData = new VehicleData()
            {
                VinNumber = GetString(driver, "vinfoBasic_c0"),
                Model = GetString(driver, "vinfoBasic_c1"),
                DateOfProduction = GetString(driver, "vinfoBasic_c2"),
                Year = Convert.ToInt32(GetString(driver, "vinfoBasic_c3")),
                SalesType = GetString(driver, "vinfoBasic_c4"),
                EngineCode = GetString(driver, "vinfoBasic_c5"),
                TransmissionCode = GetString(driver, "vinfoBasic_c6"),
                AxleDrive = GetString(driver, "vinfoBasic_c7"),
                Equipment = GetString(driver, "vinfoBasic_c8"),
                RoofColor = GetString(driver, "vinfoBasic_c9"),
                ExteriorColorAndPaintCode = GetString(driver, "vinfoBasic_c10"),
                Done = true
            };

            for (int i = 0; i < 10; i++) {

                MajorCategory majorCategory = new MajorCategory()
                {
                    Type = driver.FindElement(By.Id($"mainGroupsTable_{1}")).FindElement(By.CssSelector("div[class=\"p5_table_cell p5t6_caption\"]")).Text,
                    Done = true,
                    VehicleDataId = -1,
                };

                driver.FindElement(By.Id($"mainGroupsTable_{1}")).FindElement(By.CssSelector("div[class=\"p5_table_cell p5t6_caption\"]")).Click();



            }
            //var vehicleData = await _requestManager.Get($"https://www.partslink24.com/p5vwag/extern/directAccess?lang=en&serviceName=vw_parts&q={"WVWA12608CT025946"}&p5v=1.7.17&_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");

            //var majorCategory = await _requestManager.Get($"https://www.partslink24.com/p5vwag/extern/groups/vin_maingroups?lang=en&serviceName=vw_parts&upds=2022-11-11--15-05&vin={"WVWA12608CT025946"}&_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");

            //var minorCategory = await _requestManager.Get($"");

            driver.Dispose();

        }
    }
}
