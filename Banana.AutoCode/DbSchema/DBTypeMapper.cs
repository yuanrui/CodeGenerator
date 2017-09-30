using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.DbSchema
{
    public static class DBTypeMapper
    {
        /// <summary>
        /// 将数据库类型映射为.Net类型
        /// 参考网址:http://msdn.microsoft.com/zh-cn/library/cc716729(v=vs.100).aspx
        /// </summary>
        /// <param name="dbType">数据库类型名</param>
        /// <returns>.Net类型</returns>
        public static Type DBTypeToCodeType(String dbType)
        {
            switch (dbType.ToLowerInvariant())
            {
                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                case "time":
                    return typeof(DateTime);
                case "bit":
                case "boolean":
                    return typeof(bool);
                case "tinyint":
                    return typeof(byte);
                case "smallint":
                    return typeof(Int16);
                case "int":
                case "integer":
                    return typeof(int);
                case "bigint":
                case "long":
                    return typeof(long);
                case "decimal":
                case "smallmoney":
                case "money":
                case "numeric":
                    return typeof(decimal);
                case "real":
                    return typeof(Single);
                case "float":
                    return typeof(double);
                case "char":
                case "nchar":
                case "varchar":
                case "nvarchar":
                case "text":
                case "ntext":
                    return typeof(string);
                case "blob":
                case "binary":
                case "image":
                case "rowversion":
                case "timestamp":
                case "varbinary":
                    return typeof(byte[]);
                case "uniqueidentifier":
                    return typeof(Guid);
                default:
                    return dbType.Contains("int") ? typeof(int) : typeof(string);
            }
        }

        /// <summary>
        /// 将数据库类型映射为.Net类型名
        /// 参考网址:http://msdn.microsoft.com/zh-cn/library/cc716729(v=vs.100).aspx
        /// </summary>
        /// <param name="dbType">数据库类型名</param>
        /// <returns>.Net类型名</returns>
        public static String DBTypeToCodeTypeName(String dbType)
        {
            switch (dbType.ToLowerInvariant())
            {
                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                case "time":
                    return "DateTime";
                case "bit":
                case "boolean":
                    return "bool";
                case "tinyint":
                    return "byte";
                case "smallint":
                    return "Int16";
                case "int":
                case "integer":
                    return "int";
                case "bigint":
                case "long":
                    return "long";
                case "decimal":
                case "smallmoney":
                case "money":
                case "numeric":
                    return "decimal";
                case "real":
                    return "Single";
                case "float":
                    return "double";
                case "char":
                case "nchar":
                case "varchar":
                case "nvarchar":
                case "text":
                case "ntext":
                    return "string";
                case "blob":
                case "binary":
                case "image":
                case "rowversion":
                case "timestamp":
                case "varbinary":
                    return "byte[]";
                case "uniqueidentifier":
                    return "Guid";
                default:
                    return dbType.Contains("int") ? "int" : "string";
            }
        }
    }
}
