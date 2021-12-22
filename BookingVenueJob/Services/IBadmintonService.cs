using BookingVenueJob.Model;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BookingVenueJob.Services
{
    public interface IBadmintonService
    {
        void Model();

        void AutoBook();

        Task BookBadmintonNewPageAsync(int periodIndex, int venueIndex);

        void BookBadmintonByUrl();

        /// <summary>
        /// 操作登入並使用網址訂場地
        /// </summary>
        void BookByUrl();

        Task HtmlSampleAsync();
    }
    public class BadmintonService : IBadmintonService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ProjectSetting _projectSetting;

        public BadmintonService(IHttpClientFactory httpClientFactory,
                              IOptions<ProjectSetting> projectSetting)
        {
            _httpClientFactory = httpClientFactory;
            _projectSetting = projectSetting.Value;
        }

        public void Model()
        {
            Uri uri = new Uri(_projectSetting.SportCenterUrl.LoginPage);
            string bookDay = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");

            ChromeOptions options = new ChromeOptions();
            IWebDriver web = new ChromeDriver(options);
            //設定cookie，如PTT 八卦版需要new Cookie("over18","1")
            //web.Manage().Cookies.AddCookie(new Cookie(key, value));
            web.Navigate().GoToUrl(uri);
            WebDriverWait wait = new WebDriverWait(web, TimeSpan.FromSeconds(10000));
            //等待畫面跑出
            web.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10000);
            //透過javascript的語法，新增分頁並拜訪網頁
            //((IJavaScriptExecutor)web).ExecuteScript($"window.open('')");
            //driver.SwitchTo().Window(driver.WindowHandles.Last()); //將控制權更改為最後一個分頁
            //driver.SwitchTo().Window(driver.WindowHandles[driver.WindowHandles.Count - 1]) //也可以這樣寫
            //截圖
            //Screenshot screenshot = ((ITakesScreenshot)selenium).GetScreenshot();
            //screenshot.SaveAsFile("beauty.jpg", ScreenshotImageFormat.Jpeg);

            //輸入帳號
            wait.Until(drv => drv.FindElement(By.Id("txt_Account")));
            IWebElement inputAccount = web.FindElement(By.Id("txt_Account"));
            inputAccount.Clear();
            inputAccount.SendKeys(_projectSetting.Account.User);

            //輸入密碼
            wait.Until(drv => drv.FindElement(By.Id("txt_Pass")));
            IWebElement inputPassword = web.FindElement(By.Id("txt_Pass"));
            inputPassword.Clear();
            inputPassword.SendKeys(_projectSetting.Account.Password);

            //點擊登入 跳轉到首頁
            wait.Until(drv => drv.FindElement(By.XPath("/html/body/div[1]/div[1]/div[1]/table[2]/tbody/tr[1]/td[1]/div[1]/table[1]/tbody/tr[3]/td[1]/input")));
            IWebElement loginButton = web.FindElement(By.XPath("/html/body/div[1]/div[1]/div[1]/table[2]/tbody/tr[1]/td[1]/div[1]/table[1]/tbody/tr[3]/td[1]/input")); //By.XPath("/html/body/div[2]/form/table/tbody/tr[4]/td[2]/input")
            loginButton.Click();


            //點擊場地預約
            wait.Until(drv => drv.FindElement(By.XPath("html/body/div[1]/div[1]/div[1]/table[2]/tbody/tr[2]/td[1]/table[1]/tbody/tr[2]/td[2]/div")));
            IWebElement bookButton = web.FindElement(By.XPath("html/body/div[1]/div[1]/div[1]/table[2]/tbody/tr[2]/td[1]/table[1]/tbody/tr[2]/td[2]/div"));
            bookButton.Click();

            //場地預約 同意畫面
            wait.Until(drv => drv.FindElement(By.XPath("html/body/div[1]/div[1]/div[1]/div[1]/table[1]/tbody/tr[1]/td[1]/table[1]/tbody/tr[1]/td[1]/div")));
            IWebElement badmintonButton = web.FindElement(By.XPath("html/body/div[1]/div[1]/div[1]/div[1]/table[1]/tbody/tr[1]/td[1]/table[1]/tbody/tr[1]/td[1]/div"));
            badmintonButton.Click();

            //場地預約 同意畫面
            wait.Until(drv => drv.FindElement(By.Id("But_agree")));
            IWebElement agreeButton = web.FindElement(By.Id("But_agree"));
            agreeButton.Click();

            //場地預約 選擇日期
            wait.Until(drv => drv.FindElement(By.Id("DropDownList_SearchDate")));
            IWebElement droplist = web.FindElement(By.Id("DropDownList_SearchDate"));
            SelectElement selectElement = new SelectElement(droplist);
            selectElement.SelectByValue(bookDay);

            //場地預約選擇羽球C 18
            //wait.Until(drv => drv.FindElement(By.XPath("html/body/div[1]/div[1]/div[1]/div[3]/table[1]/tbody/tr[3]/td[1]/table[1]/tbody/tr[25]/td[4]/img")));
            //IWebElement timeButton = web.FindElement(By.XPath("html/body/div[1]/div[1]/div[1]/div[3]/table[1]/tbody/tr[3]/td[1]/table[1]/tbody/tr[25]/td[4]/img"));
            wait.Until(drv => drv.FindElement(By.XPath("html/body/div[1]/div[1]/div[1]/div[3]/table[1]/tbody/tr[3]/td[1]/table[1]/tbody/tr[23]/td[4]/img")));
            IWebElement timeButton = web.FindElement(By.XPath("html/body/div[1]/div[1]/div[1]/div[3]/table[1]/tbody/tr[3]/td[1]/table[1]/tbody/tr[23]/td[4]/img"));
            timeButton.Click();

            //場地預約 確定預約
            IWebElement confirmBookButton = web.FindElement(By.Id("btn_PlaceBook"));
            confirmBookButton.Click();

            string page = web.PageSource;
            string url = web.Url;

            web.Quit();
        }

        public void AutoBook()
        {
            Task[] bookAction = new Task[12];
            int taskIndex = 0;
            //預約場地A~F 18~20
            for (int periodIndex = 25; periodIndex <= 27; periodIndex += 2)
            {
                for (int venueIndex = 2; venueIndex <= 7; venueIndex++)
                {
                    bookAction[taskIndex] = BookBadmintonNewPageAsync(periodIndex, venueIndex);
                    taskIndex++;
                }
            }
            Task.WaitAll(bookAction);
        }

        public async Task BookBadmintonNewPageAsync(int periodIndex, int venueIndex)
        {
            await Task.Delay(1);

            Uri uri = new Uri(_projectSetting.SportCenterUrl.LoginPage);
            string bookDay = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");

            ChromeOptions options = new ChromeOptions();
            IWebDriver web = new ChromeDriver(options);
            web.Navigate().GoToUrl(uri);
            WebDriverWait wait = new WebDriverWait(web, TimeSpan.FromSeconds(10000));
            //等待畫面跑出
            web.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10000);

            Login(web, wait);

            string uriStr = _projectSetting.SportCenterUrl.BookPeriodPage + bookDay;
            uri = new Uri(uriStr);
            web.Navigate().GoToUrl(uri);

            //場地預約
            wait.Until(drv => drv.FindElement(By.XPath(string.Format(_projectSetting.DomPath.ImgButton, periodIndex, venueIndex))));
            IWebElement timeButton = web.FindElement(By.XPath(string.Format(_projectSetting.DomPath.ImgButton, periodIndex, venueIndex)));
            string title = timeButton.GetAttribute("title");
            if (title == "不可預約")
            {
                web.Quit();
                return;
            }
            timeButton.Click();

            ////場地預約 確定預約
            wait.Until(drv => drv.FindElement(By.Id("btn_PlaceBook")));
            IWebElement confirmBookButton = web.FindElement(By.Id("btn_PlaceBook"));
            confirmBookButton.Click();

            web.Quit();
        }

        public void BookBadmintonByUrl()
        {
            Uri uri = new Uri(_projectSetting.SportCenterUrl.LoginPage);
            string bookDay = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");

            ChromeOptions options = new ChromeOptions();
            IWebDriver web = new ChromeDriver(options);
            web.Navigate().GoToUrl(uri);
            WebDriverWait wait = new WebDriverWait(web, TimeSpan.FromSeconds(10000));
            //等待畫面跑出
            web.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10000);

            Login(web, wait);

            for (int periodIndex = 25; periodIndex <= 27; periodIndex += 2)
            {
                for (int venueIndex = 2; venueIndex <= 7; venueIndex++)
                {
                    string uriStr = _projectSetting.SportCenterUrl.BookPeriodPage + bookDay;
                    uri = new Uri(uriStr);
                    web.Navigate().GoToUrl(uri);

                    //場地預約
                    wait.Until(drv => drv.FindElement(By.XPath(string.Format("//*[@id='Div_PlaceBookingList']/table/tbody/tr[3]/td/table/tbody/tr[{0}]/td[{1}]/img", periodIndex, venueIndex))));
                    IWebElement timeButton = web.FindElement(By.XPath(string.Format("//*[@id='Div_PlaceBookingList']/table/tbody/tr[3]/td/table/tbody/tr[{0}]/td[{1}]/img", periodIndex, venueIndex)));
                    string title = timeButton.GetAttribute("title");
                    if (title == "不可預約")
                    {
                        continue;
                    }
                    timeButton.Click();

                    //場地預約 確定預約
                    wait.Until(drv => drv.FindElement(By.Id("btn_PlaceBook")));
                    IWebElement confirmBookButton = web.FindElement(By.Id("btn_PlaceBook"));
                    confirmBookButton.Click();
                }
            }
            web.Quit();
        }

        public void BookByUrl()
        {
            Uri uri = new Uri(_projectSetting.SportCenterUrl.LoginPage);
            string bookDay = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");

            ChromeOptions options = new ChromeOptions();
            IWebDriver web = new ChromeDriver(options);
            web.Navigate().GoToUrl(uri);
            string a = web.PageSource;
            WebDriverWait wait = new WebDriverWait(web, TimeSpan.FromSeconds(10000));
            //等待畫面跑出
            web.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10000);

            Login(web, wait);

            List<string> venueNos = new List<string> { "83", "84", "1074", "1075", "87", "2225" };
            List<string> times = new List<string> { "18", "19" };
            //預約場地A~F 18~20
            foreach (string venueNo in venueNos)
            {
                foreach (string time in times)
                {
                    string uriStr = string.Format(_projectSetting.SportCenterUrl.BookingUrl, venueNo, bookDay, time);
                    uri = new Uri(uriStr);
                    web.Navigate().GoToUrl(uri);
                }
            }
            web.Quit();
        }

        /// <summary>
        /// 登入操作
        /// </summary>
        /// <param name="web"></param>
        /// <param name="wait"></param>
        public void Login(IWebDriver web, WebDriverWait wait)
        {
            //輸入帳號
            wait.Until(drv => drv.FindElement(By.Id("txt_Account")));
            IWebElement inputAccount = web.FindElement(By.Id("txt_Account"));
            inputAccount.Clear();
            inputAccount.SendKeys(_projectSetting.Account.User);

            //輸入密碼
            wait.Until(drv => drv.FindElement(By.Id("txt_Pass")));
            IWebElement inputPassword = web.FindElement(By.Id("txt_Pass"));
            inputPassword.Clear();
            inputPassword.SendKeys(_projectSetting.Account.Password);

            //點擊登入
            wait.Until(drv => drv.FindElement(By.XPath("/html/body/div[1]/div[1]/div[1]/table[2]/tbody/tr[1]/td[1]/div[1]/table[1]/tbody/tr[3]/td[1]/input")));
            IWebElement loginButton = web.FindElement(By.XPath("/html/body/div[1]/div[1]/div[1]/table[2]/tbody/tr[1]/td[1]/div[1]/table[1]/tbody/tr[3]/td[1]/input"));
            loginButton.Click();
        }

        public async Task HtmlSampleAsync()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            Uri uri = new Uri(@"https://nd01.xuanen.com.tw/MobileLogin/MobileLogin");
            HttpResponseMessage response = await httpClient.GetAsync(uri);
            HttpContent content = response.Content;
        }
    }
}
