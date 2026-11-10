//------------------------------------------------------------------------------
// <copyright file="DbProviderIcons.cs">
//    Copyright (c) 2025, https://github.com/yuanrui All rights reserved.
// </copyright>
// <author>Yuan Rui</author>
// <date>2026-11-10 10:00:00</date>
//------------------------------------------------------------------------------

using Banana.AutoCode.DbSchema;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Banana.AutoCode.Core
{
    /// <summary>
    /// 数据库连接节点图标：按 ProviderName 映射到图标 Key。
    /// 图标取自 DBeaver（Apache-2.0），来源与许可见 Resources/DbIcons/SOURCES.md。
    /// </summary>
    public static class DbProviderIcons
    {
        /// <summary>
        /// 未知或不支持的 Provider 使用的默认图标，与连接树原有根节点图标一致。
        /// </summary>
        public const String DefaultKey = "databases";

        public const String MySqlKey = "db_mysql";
        public const String PostgreSqlKey = "db_postgresql";
        public const String SQLiteKey = "db_sqlite";
        public const String SqlServerKey = "db_sqlserver";
        public const String OracleKey = "db_oracle";

        private static readonly Dictionary<String, String> IconKeyMap = CreateIconKeyMap();

        // 位图只从资源读取一次并常驻，避免每次刷新连接树都创建新的 GDI 句柄。
        private static readonly Dictionary<String, Bitmap> IconImages = new Dictionary<String, Bitmap>
        {
            { MySqlKey, Properties.Resources.Icons_16x16_MySql },
            { PostgreSqlKey, Properties.Resources.Icons_16x16_PostgreSql },
            { SQLiteKey, Properties.Resources.Icons_16x16_SQLite },
            { SqlServerKey, Properties.Resources.Icons_16x16_SqlServer },
            { OracleKey, Properties.Resources.Icons_16x16_Oracle },
        };

        private static readonly Bitmap DefaultBitmap = Properties.Resources.Icons_16x16_Datas;

        /// <summary>
        /// 未知 Provider 使用的默认图标。
        /// </summary>
        public static Bitmap DefaultImage
        {
            get { return DefaultBitmap; }
        }

        /// <summary>
        /// 返回 ProviderName 对应的 16×16 图标；未注册的 Provider 回退到默认图标。
        /// 返回的是静态缓存的位图实例，调用方不要释放。
        /// </summary>
        public static Bitmap GetImage(String providerName)
        {
            Bitmap image;

            if (IconImages.TryGetValue(GetIconKey(providerName), out image))
            {
                return image;
            }

            return DefaultBitmap;
        }

        /// <summary>
        /// 数据库类型图标，Key 为 ImageList 中的图标名。
        /// </summary>
        public static IEnumerable<KeyValuePair<String, Bitmap>> Images
        {
            get { return IconImages; }
        }

        /// <summary>
        /// 返回 ProviderName 对应的图标 Key；未注册的 Provider 回退到默认图标，不抛异常。
        /// </summary>
        public static String GetIconKey(String providerName)
        {
            String key;

            if (!String.IsNullOrWhiteSpace(providerName)
                && IconKeyMap.TryGetValue(providerName.Trim(), out key))
            {
                return key;
            }

            return DefaultKey;
        }

        private static Dictionary<String, String> CreateIconKeyMap()
        {
            // net40 下 MySqlConnector 与 MySql.Data.MySqlClient 是同一个值，用索引器赋值避免重复键。
            var map = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase);

            map[DbProviderConfig.NS.MySql_Data_MySqlClient] = MySqlKey;
            map[DbProviderConfig.NS.MySqlConnector] = MySqlKey;
            map[DbProviderConfig.NS.Npgsql] = PostgreSqlKey;
            map[DbProviderConfig.NS.System_Data_SQLite] = SQLiteKey;
            map[DbProviderConfig.NS.System_Data_SqlClient] = SqlServerKey;
            map[DbProviderConfig.NS.Microsoft_Data_SqlClient] = SqlServerKey;
            map[DbProviderConfig.NS.Oracle_ManagedDataAccess_Client] = OracleKey;

            // 「数据库类型」下拉显示的是 ProviderName 的友好名称，这里一并登记，
            // 保证下拉项与连接树使用同一套图标映射。
            map[DbProviderConfig.MySql] = MySqlKey;
            map[DbProviderConfig.PostgreSQL] = PostgreSqlKey;
            map[DbProviderConfig.SQLite] = SQLiteKey;
            map[DbProviderConfig.SqlServer] = SqlServerKey;
            map[DbProviderConfig.Oracle] = OracleKey;

            return map;
        }
    }
}
