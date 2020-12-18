//------------------------------------------------------------------------------
// <copyright file="StringExtension.cs" company="CQ Ebos Co., Ltd.">
//    Copyright (c) 2020, CQ Ebos Co., Ltd. All rights reserved.
// </copyright>
// <author>Yuan Rui</author>
// <email>yuanrui@live.cn</email>
// <date>2020/12/15 9:42:12</date>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.AutoCode
{
    public static class StringExtension
    {
        public static String ToPascalCase(this String input)
        {
            return ToPascalCase(input, '_');
        }

        private static String ToPascalCase(String input, params Char[] separator)
        {
            if (String.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            
            String[] words = input.Split(separator);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length == 0) continue;

                Char firstChar = Char.ToUpper(words[i][0]);
                String rest = String.Empty;
                if (words[i].Length > 1)
                {
                    rest = words[i].Substring(1);
                    if (rest.IsAllUpperCase())
                    {
                        rest = rest.ToLower();
                    }
                }
                words[i] = firstChar + rest;
            }

            return String.Join(String.Empty, words);
        }

        public static String ToCamelCase(this String input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            String[] words = input.Split('_');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length == 0) continue;
                Char firstChar = Char.ToUpper(words[i][0]);
                if (i == 0)
                {
                    firstChar = Char.ToLower(words[i][0]);
                }

                String rest = String.Empty;
                if (words[i].Length > 1)
                {
                    rest = words[i].Substring(1);
                    if (rest.IsAllUpperCase())
                    {
                        rest = rest.ToLower();
                    }
                }
                words[i] = firstChar + rest;
            }

            return String.Join(String.Empty, words);
        }

        public static Boolean IsAllUpperCase(this String input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            var length = input.Length;
            var count = 0;

            for (int i = 0; i < input.Length; i++)
            {
                var word = input[i];
                if (word >= 'A' && word <= 'Z')
                {
                    count++;
                }
            }

            return count == length;
        }

        public static Boolean IsAnyUpperCase(this String input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            for (int i = 0; i < input.Length; i++)
            {
                var word = input[i];
                if (word >= 'A' && word <= 'Z')
                {
                    return true;
                }
            }

            return false;
        }

        public static Boolean IsAllLowerCase(this String input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            var length = input.Length;
            var count = 0;

            for (int i = 0; i < input.Length; i++)
            {
                var word = input[i];
                if (word >= 'a' && word <= 'a')
                {
                    count++;
                }
            }

            return count == length;
        }

        public static Boolean IsAnyLowerCase(this String input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            for (int i = 0; i < input.Length; i++)
            {
                var word = input[i];
                if (word >= 'a' && word <= 'a')
                {
                    return true;
                }
            }

            return false;
        }
    }
}
