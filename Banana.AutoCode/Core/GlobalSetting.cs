using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.Core
{
    public class GlobalSetting
    {
        public static string[] RemovePrefixList
        {
            get
            {
                var input = ConfigurationManager.AppSettings["removePrefix"];

                if (string.IsNullOrWhiteSpace(input))
                {
                    return new string[0];
                }

                return input.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static string[] RemoveSuffixList
        {
            get
            {
                var input = ConfigurationManager.AppSettings["removeSuffix"];
                
                if (string.IsNullOrWhiteSpace(input))
                {
                    return new string[0];
                }

                return input.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
