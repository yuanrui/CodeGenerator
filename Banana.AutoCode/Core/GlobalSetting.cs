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
                    return new string[1] { "T_" };
                }

                return input.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries)
                    .OrderByDescending(m => m.Length)
                    .ToArray();
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
