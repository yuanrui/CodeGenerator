using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Banana.AutoCode.Resources;

namespace Banana.AutoCode
{
    public class CultureHelper
    {
        public static String[] CultureList = new String[] { "en-US", "zh-CN" };

        public static void SetCulture()
        {
            String cultureName = ConfigurationManager.AppSettings["culture"];
            if (!CultureIsSupport(cultureName))
            {
                cultureName = CultureInfo.CurrentCulture.Name;

                if (!CultureIsSupport(cultureName))
                {
                    cultureName = "en-US";
                }
            }

            SetCulture(cultureName);
        }

        public static void SetCulture(String cultureName)
        {
            CultureInfo cultureInfo = new CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            Localization.Culture = cultureInfo;
        }

        public static Boolean CultureIsSupport(String cultureName)
        {
            if (String.IsNullOrWhiteSpace(cultureName) || !CultureList.Any(m => m == cultureName))
            {
                return false;
            }

            return true;
        }
    }
}
