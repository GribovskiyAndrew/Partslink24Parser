using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Partslink24Parser
{
    public class RequestManager
    {
        private readonly HttpClient _httpClient;

        private Dictionary<string, string> headers = new Dictionary<string, string>();
        public RequestManager()
        {
            var handler = new HttpClientHandler();
            handler.UseCookies = false;

            //handler.Proxy = new WebProxy()
            //{
            //    Address = new Uri($"http://109.94.163.66:52218"),
            //    BypassProxyOnLocal = false,
            //    UseDefaultCredentials = false,

            //    Credentials = new NetworkCredential(userName: "fqY7Mpqi", password: "Jw8x9jdn")
            //};

            handler.AutomaticDecompression = ~DecompressionMethods.None;

            _httpClient = new HttpClient(handler);

            //headers.Add("sec-ch-ua", "\"Google Chrome\";v=\"107\", \"Chromium\";v=\"107\", \"Not=A?Brand\";v=\"24\"");
            //headers.Add("X-Client-Version", "4.6.0");
            //headers.Add("sec-ch-ua-mobile", "?0");
            //headers.Add("X-Api", "CustomerAPI");
            //headers.Add("X-Client-App", "web");
            //headers.Add("Authorization", "Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6ImRjMzdkNTkzNjVjNjIyOGI4Y2NkYWNhNTM2MGFjMjRkMDQxNWMxZWEiLCJ0eXAiOiJKV1QifQ.eyJuYW1lIjoiSmFjayBsaWFtIiwicGljdHVyZSI6Imh0dHBzOi8vbGgzLmdvb2dsZXVzZXJjb250ZW50LmNvbS9hL0FMbTV3dTE0Vk5LTnNjbHFGV1lTaTBDY25wWG1UOHZMX2NMOGxMRHRZVlFIPXM5Ni1jIiwiY2lkIjozMTU1NTkyLCJpc3MiOiJodHRwczovL3NlY3VyZXRva2VuLmdvb2dsZS5jb20vYm9vZG1vLXRlc3QiLCJhdWQiOiJib29kbW8tdGVzdCIsImF1dGhfdGltZSI6MTY2NzI0NzAxMywidXNlcl9pZCI6IjZTMmM3S2Q0OThRNFZGN3JmNTdxWkNBdFkxVDIiLCJzdWIiOiI2UzJjN0tkNDk4UTRWRjdyZjU3cVpDQXRZMVQyIiwiaWF0IjoxNjY3MzA4ODAzLCJleHAiOjE2NjczMTI0MDMsImVtYWlsIjoiamFja2xpYW0yNTIwQGdtYWlsLmNvbSIsImVtYWlsX3ZlcmlmaWVkIjp0cnVlLCJmaXJlYmFzZSI6eyJpZGVudGl0aWVzIjp7Imdvb2dsZS5jb20iOlsiMTE4Mzc2MTkzMzg3MjUzNjg5MzM0Il0sImVtYWlsIjpbImphY2tsaWFtMjUyMEBnbWFpbC5jb20iXX0sInNpZ25faW5fcHJvdmlkZXIiOiJwYXNzd29yZCJ9fQ.B2ugUK1w7YlASACE5BG46eNZK2K5zPK_E02eUiGQ1F9fY-jY_qMwp-cupcOoAvJXMA9oQnpWZj_zujnF3Ho4TCB9TVq4P3OkV3RvETPrA3nL-KX7QoXeWNPm5rKBUgFN73zMhhfS1NFdaLF8Xa8zY2RQ5C9fETSYj19_iEwQfmY6dv21fauqA2L983d03i7uSfpVV-wlz2OqaKJPFaCNZhgN7zjCno2ee0R1a9GHIlGVtBDIWVVWoEymUOpv6Z9IxeB3Q9mhOyNaXoFAF2NXFnvUTurwwm3a5b6sxQbVVL2iL3EHUZJzQcfAGc3rnP1CUiREnlM9vrEodmZiwfyzSA");
            //headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36");
            //headers.Add("Accept", "application/json, text/plain, */*");
            //headers.Add("Referer", "https://boodmo.com/");
            //headers.Add("X-Client-Id", "e3d9bcb2915ce83b40a3a90724e12b0b");
            //headers.Add("X-Boo-Sign", "a2f0c47791f99d947b67ed3aa336023a");
            //headers.Add("X-Date", "2022-11-01T13:20:19.382Z");
            //headers.Add("Accept-Version", "v1");
            //headers.Add("X-Client-Build", "221026.1443");
            //headers.Add("sec-ch-ua-platform", "\"Windows\"");
        }

        public async Task SaveImage(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            string file = "C:\\Users\\lifebookE\\source\\repos\\Partslink24Parser\\Partslink24Parser\\Images" + name;

            string url = "https://boodmo.com/media/cache/part_zoom_horizontal" + name;

            await SaveImage(url, file);
        }

        private async Task SaveImage(string url, string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            if (string.IsNullOrEmpty(url))
                return;

            if (File.Exists(path))
                return;

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                AddHeaders(request);

                var response = await _httpClient.SendAsync(request);

                var bytes = await response.Content.ReadAsByteArrayAsync();

                await File.WriteAllBytesAsync(path, bytes);
            }
        }

        private void AddHeaders(HttpRequestMessage request)
        {
            //request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Google Chrome\";v=\"107\", \"Chromium\";v=\"107\", \"Not=A?Brand\";v=\"24\"");
            //request.Headers.TryAddWithoutValidation("X-Client-Version", "4.6.0");
            //request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            //request.Headers.TryAddWithoutValidation("X-Api", "CustomerAPI");
            //request.Headers.TryAddWithoutValidation("X-Client-App", "web");
            //request.Headers.TryAddWithoutValidation("Authorization", "Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6ImRjMzdkNTkzNjVjNjIyOGI4Y2NkYWNhNTM2MGFjMjRkMDQxNWMxZWEiLCJ0eXAiOiJKV1QifQ.eyJuYW1lIjoiQU1nbCBHTGFtIiwicGljdHVyZSI6Imh0dHBzOi8vbGgzLmdvb2dsZXVzZXJjb250ZW50LmNvbS9hL0FJdGJ2bW0yY2RXRGt2Vjl1eGFZVDlKMThrcTRTbnZoNFZZUmFTUVZUdmpUPXM5Ni1jIiwiY2lkIjoyOTcxMDQ5LCJpc3MiOiJodHRwczovL3NlY3VyZXRva2VuLmdvb2dsZS5jb20vYm9vZG1vLXRlc3QiLCJhdWQiOiJib29kbW8tdGVzdCIsImF1dGhfdGltZSI6MTY2NzU0OTU4OCwidXNlcl9pZCI6InY2Q0NWTWY0TWZlSjlOT1EwZllCUWM0QVRpVDIiLCJzdWIiOiJ2NkNDVk1mNE1mZUo5Tk9RMGZZQlFjNEFUaVQyIiwiaWF0IjoxNjY3NTU0NDc2LCJleHAiOjE2Njc1NTgwNzYsImVtYWlsIjoiYW1nbC50YXBraTIwMjBAZ21haWwuY29tIiwiZW1haWxfdmVyaWZpZWQiOnRydWUsImZpcmViYXNlIjp7ImlkZW50aXRpZXMiOnsiZ29vZ2xlLmNvbSI6WyIxMDQ1NDMwODAzOTMxMTkzNzE0NDMiXSwiZW1haWwiOlsiYW1nbC50YXBraTIwMjBAZ21haWwuY29tIl19LCJzaWduX2luX3Byb3ZpZGVyIjoicGFzc3dvcmQifX0.dE6DZxRSJDa0vqJpZlKXiFg4RCsW2-fWsM9q3Fop-4g-_1AEyIzPo7E7e0cjQvUK9Lks8mPokNFFLRTLOV2iXfyVo-5R8RuXQbXe_j7TtFrF-l6crMgQ4ugiRC7rN1qhb6ZVoJ3lpI5Br_GWzTGigjX3mNCueCzFqgAw_miJyr8odO_ijcSGFiDbL07zyyMj9zmqszViN5JfXOcs-c_9_c4hm_kwDhX2umseR0vA8rVm2pnXJsKkR95C96Qs_b6My1vPSgECwop_hyUE0Hzw5VZyhgWyoN0aKJOiZrckmKU6EH5as7I9ZLX6hgnpfQ4t3AsnuD71r8qQ_SuperI3Fw");
            //request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36");
            //request.Headers.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
            //request.Headers.TryAddWithoutValidation("Referer", "https://boodmo.com/");
            //request.Headers.TryAddWithoutValidation("X-Client-Id", "e3d9bcb2915ce83b40a3a90724e12b0b");
            //request.Headers.TryAddWithoutValidation("X-Boo-Sign", "7efcce4312df0160126e7082dd659413");
            //request.Headers.TryAddWithoutValidation("X-Date", "2022-11-04T09:34:31.315Z");
            //request.Headers.TryAddWithoutValidation("Accept-Version", "v1");
            //request.Headers.TryAddWithoutValidation("X-Client-Build", "221026.1443");
            //request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
        }

        public void AddHeaders(Dictionary<string, string> _headers)
        {
            headers = _headers;
        }

        public async Task<JObject> Get(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                foreach (var header in headers)
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);

                //AddHeaders(request);

                var response = await _httpClient.SendAsync(request);

                var resp = await GetResponse(response);

                if( resp == null)
                {

                }
                return await GetResponse(response);
            }
        }

        public void AddHeaders()
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

                //_headers.Add(prop.Name, prop.Value.ToString());
            }

            _requestManager.AddHeaders(_headers);
        }

        public async Task<JObject> Post(string url, object data)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                foreach (var header in headers)
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);

                //AddHeaders(request);

                var content = JsonSerializer.Serialize(data);
                var buffer = Encoding.UTF8.GetBytes(content);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=UTF-8");

                request.Content = byteContent;

                var response = await _httpClient.SendAsync(request);

                return await GetResponse(response);
            }
        }

        public async Task<JObject> GetResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync();

                JObject result = JObject.Parse(responseText);

                return result;
            }
            else
                return null;
        }
    }
}
