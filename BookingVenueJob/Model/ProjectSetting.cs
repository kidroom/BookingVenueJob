using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingVenueJob.Model
{
    public class ProjectSetting
    {
        public Account Account { get; set; }
        public SportCenterUrl SportCenterUrl { get; set; }
        public DomPath DomPath { get; set; }
    }

    /// <summary>
    /// 帳密設定
    /// </summary>
    public class Account
    {
        public string User { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// 網址
    /// </summary>
    public class SportCenterUrl
    {
        /// <summary>
        /// 登入畫面
        /// </summary>
        public string LoginPage { get; set; }

        /// <summary>
        /// 選定場地時間畫面
        /// </summary>
        public string BookPeriodPage { get; set; }

        /// <summary>
        /// 下訂網址<br/>
        /// {0} 羽A:83 羽B:84 羽C:1074 羽D:1075 羽E:87 羽F:2225<br/>
        /// {1} 日期(yyyy-MM-dd)<br/>
        /// {2} 時間(HH)<br/>
        /// </summary>
        public string BookingUrl { get; set; }
    }

    /// <summary>
    /// DOM XPath
    /// </summary>
    public class DomPath
    {
        /// <summary>
        /// 圖片按鍵<br/>
        /// {0}:25是18~19時，27是19~20時。<br/>
        /// {1}:2~7 是 A場地~F場地。
        /// </summary>
        public string ImgButton { get; set; }
    }
}
