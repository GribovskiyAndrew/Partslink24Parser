using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using LogLevel = OpenQA.Selenium.LogLevel;
using Newtonsoft.Json.Linq;
using Partslink24Parser.Entities;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;

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
            IWebElement element = null;
            try
            {
                element = driver.FindElement(by);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
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

            if (isElementPresent(driver, By.Id("squeezeout-login-btn")))
            {
                driver.FindElement(By.Id("squeezeout-login-btn")).Click();
            }

            driver.FindElement(By.CssSelector("input[placeholder=\"Search VIN\"]")).SendKeys("WVWA12608CT025946");

            driver.FindElement(By.CssSelector("div[class=\"search-txt\"]")).Click();

            Dictionary<string, string> _headers = new Dictionary<string, string>();

            driver.FindElement(By.CssSelector(".p5_vehicle_info_vin"));





            var logs = driver.Manage().Logs;

            var perf = logs.GetLog(LogType.Performance);

            var item = perf.Select(x => x.Message).Where(x => x != null && x.Contains("/directAccess") && x.Contains("referer") && x.Contains("cookie") && x.Contains("authority") && x.Contains("x-requested-with")).FirstOrDefault();

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

            //string cookie = $"_gcl_au={driver.Manage().Cookies.GetCookieNamed("_gcl_au").Value}; _fbp={driver.Manage().Cookies.GetCookieNamed("_fbp").Value}; PL24SESSIONID={driver.Manage().Cookies.GetCookieNamed("PL24SESSIONID").Value}; pl24LoggedInTrail={driver.Manage().Cookies.GetCookieNamed("pl24LoggedInTrail").Value}; PL24TOKEN={driver.Manage().Cookies.GetCookieNamed("PL24TOKEN").Value}";

            //_headers["cookie"] = cookie;

            //_requestManager.AddHeaders(_headers);

            //var token = await _requestManager.Post("https://www.partslink24.com/auth/ext/api/1.1/authorize",
            //    new { serviceNames = new string[] { "cart", "config_parts", "pl24-full-vin-data", "pl24-sendbtmail", "pl24-orderbridge", "vw_parts" }, withLogin = true});

            //var authorization = token["token_type"].ToString() + " " + token["access_token"].ToString();

            string cookie = $"_gcl_au={driver.Manage().Cookies.GetCookieNamed("_gcl_au").Value}; _fbp={driver.Manage().Cookies.GetCookieNamed("_fbp").Value}; PL24SESSIONID={driver.Manage().Cookies.GetCookieNamed("PL24SESSIONID").Value}; pl24LoggedInTrail={driver.Manage().Cookies.GetCookieNamed("pl24LoggedInTrail").Value}; PL24TOKEN={driver.Manage().Cookies.GetCookieNamed("PL24TOKEN").Value}";

            _headers["cookie"] = cookie;

            _requestManager.AddHeaders(_headers);

            var vehicle = await _requestManager.Get($"https://www.partslink24.com/p5vwag/extern/directAccess?lang=en&serviceName=vw_parts&q={"WVWA12608CT025946"}&p5v=1.7.18&_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
            
            var vehicleValues = vehicle["data"]["segments"]["vinfoBasic"]["records"];

            VehicleData vehicleData = new VehicleData()
            {
                VinNumber = vehicleValues[0]["values"]["value"].ToString(),
                Model = vehicleValues[1]["values"]["value"].ToString(),
                DateOfProduction = vehicleValues[2]["values"]["value"].ToString(),
                Year = Convert.ToInt32(vehicleValues[3]["values"]["value"]),
                SalesType = vehicleValues[4]["values"]["value"].ToString(),
                EngineCode = vehicleValues[5]["values"]["value"].ToString(),
                TransmissionCode = vehicleValues[6]["values"]["value"].ToString(),
                AxleDrive = vehicleValues[7]["values"]["value"].ToString(),
                Equipment = vehicleValues[8]["values"]["value"].ToString(),
                RoofColor = vehicleValues[9]["values"]["value"].ToString(),
                ExteriorColorAndPaintCode = vehicleValues[10]["values"]["value"].ToString(),
                Done = true
            };

            await _context.VehicleData.AddAsync(vehicleData);

            await _context.SaveChangesAsync();

            var vehicleDataInBase = await _context.VehicleData.FirstOrDefaultAsync(x =>
                                        x.VinNumber == vehicleData.VinNumber &&
                                        x.Model == vehicleData.Model &&
                                        x.DateOfProduction == vehicleData.DateOfProduction &&
                                        x.Year == vehicleData.Year &&
                                        x.SalesType == vehicleData.SalesType &&
                                        x.EngineCode == vehicleData.EngineCode &&
                                        x.TransmissionCode == vehicleData.TransmissionCode &&
                                        x.AxleDrive == vehicleData.AxleDrive &&
                                        x.Equipment == vehicleData.Equipment &&
                                        x.RoofColor == vehicleData.RoofColor &&
                                        x.ExteriorColorAndPaintCode == vehicleData.ExteriorColorAndPaintCode
                                        );

            logs = driver.Manage().Logs;

            perf = logs.GetLog(LogType.Performance);

            item = perf.Select(x => x.Message).Where(x => x != null && x.Contains("/groups/vin_maingroups") && x.Contains("authorization") && x.Contains("cookie") && x.Contains("authority") && x.Contains("accept-language")).FirstOrDefault();

            result = JObject.Parse(item);

            headers = result["message"]["params"]["headers"];

            _headers = new Dictionary<string, string>();

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

            _requestManager.AddHeaders(_headers);

            var majorCategoryData = await _requestManager.Get($"https://www.partslink24.com/p5vwag/extern/groups/vin_maingroups?lang=en&serviceName=vw_parts&upds=2022-11-18--00-05&vin={"WVWA12608CT025946"}&_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");

            //List<MajorCategory> majorCategoryList = new List<MajorCategory>();

            //foreach (var data in majorCategoryData["data"]["records"])
            //{
            //    majorCategoryList.Add(new MajorCategory
            //    {
            //        Type = data["values"]["caption"].ToString(),
            //        Done = true,
            //        VehicleDataId = -1,
            //    });
            //}



            List<MajorCategory> majorCategories = new List<MajorCategory>();

            for(int i = 0; i < 10; i++)
            {
                var majorCategory = majorCategoryData["data"]["records"][i];

                majorCategories.Add(new MajorCategory
                {
                    Type = majorCategory["values"]["caption"].ToString(),
                    Done = true,
                    VehicleDataId = vehicleDataInBase.Id,
                    Path = majorCategory["link"]["path"].ToString(),
                });
            }

            await _context.MajorСategories.BulkInsertAsync(majorCategories);


            //foreach (var majorCategory in majorCategoryData["data"]["records"])
            //{
            //    majorCategories.Add(new MajorCategory
            //    {
            //        Type = majorCategory["values"]["caption"].ToString(),
            //        Done = true,
            //        VehicleDataId = -1,
            //        Path = majorCategory["link"]["path"].ToString(),
            //    });
            //};

            //List<MajorCategory> majorCategories = majorCategoryData["data"]["records"].Select(
            //                     x => new MajorCategory
            //                     {
            //                         Type = x["values"]["caption"].ToString(),
            //                         Done = true,
            //                         VehicleDataId = -1,
            //                         Path = x["link"]["path"].ToString(),
            //                     }
            //                    ).ToList();

            List<MinorCategory> minorCategoryList = new List<MinorCategory>();

            foreach (var majorCategory in majorCategories)
            {
                var majorCategoryInBase = await _context.MajorСategories.FirstOrDefaultAsync(x =>
                                        x.Type == majorCategory.Type &&
                                        x.VehicleDataId == majorCategory.VehicleDataId
                                        );

                var minorCategory = await _requestManager.Get($"https://www.partslink24.com/" + majorCategory.Path);

                foreach (var data in minorCategory["data"]["records"])
                {
                    if (data["unavailable"] != null)
                        continue;

                    minorCategoryList.Add(new MinorCategory
                    {
                        SubGroup = data["values"]["subgroup"].ToString(),
                        Illustration = data["values"]["illustrationNumber"].ToString(),
                        Description = data["values"]["captions"].ToString(),
                        Remark = data["values"]["remarks"] != null ? data["values"]["remarks"].ToString() : "-",
                        Model = data["values"]["modelDescriptions"] != null ? data["values"]["modelDescriptions"].ToString() : "-",
                        Done = true,
                        MajorCategoryId = majorCategoryInBase.Id,
                        Path = data["link"]["path"].ToString()
                    });

                }
            }

            await _context.MinorCategories.BulkInsertAsync(minorCategoryList);

            List<Part> partsList = new List<Part>();

            foreach (var minorCategory in minorCategoryList)
            {
                var minorCategoryInBase = await _context.MinorCategories.FirstOrDefaultAsync(x =>
                                        x.SubGroup == minorCategory.SubGroup &&
                                        x.Illustration == minorCategory.Illustration &&
                                        x.Description == minorCategory.Description &&
                                        x.Remark == minorCategory.Remark &&
                                        x.Model == minorCategory.Model &&
                                        x.MajorCategoryId == minorCategory.MajorCategoryId
                                        );

                var part = await _requestManager.Get($"https://www.partslink24.com/" + minorCategory.Path + "&_=" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                var imageName = minorCategory.Id + ".png";

                if (part["data"]["images"] != null)
                {
                    var imagePath = part["data"]["images"][0]["uri"].ToString() + "M&_=" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    var img = await _requestManager.Get($"https://www.partslink24.com/" + imagePath + "M&_=" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                    var base64String = img["image"].ToString();

                    byte[] imgByte = Convert.FromBase64String(base64String);

                    await File.WriteAllBytesAsync("C:\\Users\\lifebookE\\source\\repos\\Partslink24Parser\\Partslink24Parser\\Images\\" + imageName, imgByte);

                    List<Point> coordinateList = new List<Point>();

                    foreach (var i in img["hotspots"])
                    {
                        foreach (var j in i["areas"])
                        {
                            coordinateList.Add(new Point
                            {
                                Left = Convert.ToInt32(j["left"]),
                                Top = Convert.ToInt32(j["top"]),
                                Width = Convert.ToInt32(j["widht"]),
                                Height = Convert.ToInt32(j["height"]),
                                Label = i["key"].ToString(),
                                MinorCategoryId = minorCategoryInBase.Id
                            });
                        }
                    }

                    await _context.Points.BulkInsertAsync(coordinateList);
                }
                else
                {
                    imageName = "-";
                }

                foreach (var data in part["data"]["records"])
                {
                    partsList.Add(new Part
                    {
                        Position = data["values"]["pos"] != null ? data["values"]["pos"].ToString() : "-",
                        PartNumber = data["values"]["partno"] != null ? data["values"]["partno"].ToString() : "-",
                        Description = data["values"]["description"] != null ? data["values"]["description"].ToString() : "-",
                        Remark = data["values"]["remark"] != null ? data["values"]["remark"].ToString() : "-",
                        Unit = data["values"]["qty"] != null ? data["values"]["qty"].ToString() : "-",
                        Model = data["values"]["modelDescription"] != null ? data["values"]["modelDescription"].ToString() : "-",
                        Path = data["link"] != null ? data["link"]["path"].ToString() : "-",
                        Unavailable = data["unavailable"] != null ? true : false,
                        ImageName = imageName,
                        Done = true,
                        MinorCategoryId = minorCategoryInBase.Id
                    });
                }
            }

            await _context.Parts.BulkInsertAsync(partsList);

            List<PartInformation> partInformationList = new List<PartInformation>();

            foreach (var part in partsList)
            {
                var partInBase = await _context.Parts.FirstOrDefaultAsync(x =>
                                            x.Position == part.Position &&
                                            x.PartNumber == part.PartNumber &&
                                            x.Description == part.Description &&
                                            x.Remark == part.Remark &&
                                            x.Model == part.Model &&
                                            x.Unit == part.Unit &&
                                            x.Unit == part.Unit &&
                                            x.MinorCategoryId == part.MinorCategoryId
                                            );

                var partInfo = await _requestManager.Get($"https://www.partslink24.com/" + part.Path + "&_=" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                foreach (var data in partInfo["data"]["records"])
                {
                    partInformationList.Add(new PartInformation
                    {
                        PartNumber = data["values"]["pos"] != null ? data["values"]["pos"].ToString() : "-",
                        Description = data["values"]["description"] != null ? data["values"]["description"].ToString() : "-",
                        Price = data["values"]["remark"] != null ? data["values"]["remark"].ToString() : "-",
                        //Unavailable = data["unavailable"] != null ? true : false,
                        PartId = partInBase.Id
                    });
                }
            }

            await _context.PartInformation.BulkInsertAsync(partInformationList);

            driver.Dispose();

        }
    }
}
