#if !NET

using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: CLSCompliant(true)]
//[assembly: AssemblyMetadata("Serviceable", "True")]

namespace System.Reflection
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    internal sealed class AssemblyMetadataAttribute : Attribute
    {
        public AssemblyMetadataAttribute(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }
}

namespace Banana.AutoCode.Core
{
    /// <summary>
    /// 程序集编译时间Attribute
    /// 需要获取程序集的编译时间时, 在*.csproj文件中添加以下代码:
    ///   <ItemGroup>
    ///     <AssemblyAttribute Include = "Ebos.Common.Reflection.CompileTimeAttribute" >
    ///         <_Parameter1>$([System.DateTime]::Now.ToString("yyyyMMddHHmmss"))</_Parameter1>
    ///     </AssemblyAttribute>
    ///   </ItemGroup>
    /// </summary>
    /// <example>
    /// </example>
    [Obsolete("Deprecated, Suggest using System.Reflection.AssemblyMetadataAttribute")]
    [AttributeUsage(AttributeTargets.Assembly)]
    public class CompileTimeAttribute : Attribute
    {
        public CompileTimeAttribute(string value)
        {
            this.Time = DateTime.ParseExact(value, "yyyyMMddHHmmss", CultureInfo.CurrentCulture, DateTimeStyles.None);
        }

        public CompileTimeAttribute()
        {
            this.Time = DateTime.Now;
        }

        public DateTime Time { get; }

    }
}

#endif