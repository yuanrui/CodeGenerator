using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Simple.Common.Reflection
{
    public class VersionUtils
    {
        protected const string CompileTime = "CompileTime";
        protected const string yyyyMMddHHmmss = "yyyyMMddHHmmss";
        protected const string yyyyMMdd = "yyyyMMdd";
        protected const string yyMMdd = "yyMMdd";

        public readonly static DateTime VersionDate;

        public readonly static Int64 VersionNumber;

        public readonly static Version Version;

        static VersionUtils()
        {
            var assembly = Assembly.GetEntryAssembly();
            Version = assembly.GetName()?.Version ?? new Version(1, 0, 0, 0);
            VersionDate = GetAssemblyCompileTime(assembly);
            VersionNumber = Int64.Parse(VersionDate.ToString(yyyyMMddHHmmss));
        }

        private static DateTime GetLinkerTimestampLocal(Assembly assembly)
        {
            var location = assembly.Location;
            return GetLinkerTimestampLocal(location);
        }

        private static DateTime GetLinkerTimestampLocal(string filePath)
        {
            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;
            var bytes = new byte[2048];

            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                file.Read(bytes, 0, bytes.Length);
            }

            var headerPos = BitConverter.ToInt32(bytes, peHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(bytes, headerPos + linkerTimestampOffset);
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return dt.AddSeconds(secondsSince1970).ToLocalTime();
        }

        internal static DateTime GetAssemblyCompileTime(Assembly assembly)
        {
#if NETSTANDARD2_0_OR_GREATER
            var metas = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
            foreach (var attr in metas)
            {
                if (attr is AssemblyMetadataAttribute)
                {
                    var meta = attr as AssemblyMetadataAttribute;

                    if (meta.Key == CompileTime)
                    {
                        if (DateTime.TryParseExact(meta.Value, new[] { yyyyMMddHHmmss, yyyyMMdd }, CultureInfo.CurrentCulture, DateTimeStyles.None, out var time))
                        {
                            return time;
                        }
                    }
                }
            }

#endif
#if NETFRAMEWORK
            return GetLinkerTimestampLocal(assembly);
#else
            return File.GetLastWriteTime(assembly.Location);
#endif
        }

        public static string GetVersion()
        {
            return GetVersion(2);
        }

        public static string GetVersion(int fieldCount)
        {
            if (fieldCount < 0)
            {
                fieldCount = 0;
            }

            if (fieldCount > 4)
            {
                fieldCount = 4;
            }

            return $"{Version.ToString(fieldCount)}.{VersionDate.ToString(yyMMdd)}";
        }
    }

    public class VersionUtils<TypeInAssembly> : VersionUtils
    {
        public readonly new static DateTime VersionDate;

        public readonly new static Int64 VersionNumber;

        public readonly new static Version Version;

        static VersionUtils()
        {
            var assembly = typeof(TypeInAssembly).Assembly;
            VersionDate = GetAssemblyCompileTime(assembly);
            VersionNumber = Int64.Parse(VersionDate.ToString(yyyyMMddHHmmss));
        }
    }
}
