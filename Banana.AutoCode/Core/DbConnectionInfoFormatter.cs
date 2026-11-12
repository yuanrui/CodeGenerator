//------------------------------------------------------------------------------
// <copyright file="DbConnectionInfoFormatter.cs">
//    Copyright (c) 2025, https://github.com/yuanrui All rights reserved.
// </copyright>
// <author>Yuan Rui</author>
// <date>2026-11-11 12:00:00</date>
//------------------------------------------------------------------------------

using Banana.AutoCode.DbSchema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Banana.AutoCode.Core
{
    /// <summary>
    /// 人读连接信息清单的拼装结果。
    /// </summary>
    public sealed class DbConnectionInfoResult
    {
        private readonly String _text;
        private readonly Int32 _placeholderCount;

        public DbConnectionInfoResult(String text, Int32 placeholderCount)
        {
            _text = text ?? String.Empty;
            _placeholderCount = placeholderCount;
        }

        /// <summary>
        /// 一行一项的纯文本清单。
        /// </summary>
        public String Text
        {
            get { return _text; }
        }

        /// <summary>
        /// 清单中 <c>[占位符]</c> 的个数。
        /// </summary>
        public Int32 PlaceholderCount
        {
            get { return _placeholderCount; }
        }

        /// <summary>
        /// 是否含待填写项。
        /// </summary>
        public Boolean HasPlaceholders
        {
            get { return _placeholderCount > 0; }
        }
    }

    /// <summary>
    /// 把 <see cref="DbViewModel"/> 拼装成人读的连接信息清单：
    /// 每行 <c>名称: 值</c>，供复制到聊天窗口/工单里直接阅读。
    ///
    /// 与 <see cref="DbConnectionStringFormats"/> 并列：本类只产出说明性文本，
    /// 不产出任何驱动可解析的连接串，也不调用连接字符串类的拼装逻辑。
    /// 字段值为空时输出 <c>[占位符]</c>，占位符文案由调用方按当前 UI 语言解析。
    /// </summary>
    public static class DbConnectionInfoFormatter
    {
        /// <summary>
        /// 密码占位符的本地化键。
        /// </summary>
        public const String PlaceholderPassword = "ConnectionString_Placeholder_Password";

        private const String PlaceholderHost = "ConnectionString_Placeholder_Host";
        private const String PlaceholderUser = "ConnectionString_Placeholder_User";
        private const String PlaceholderServiceName = "ConnectionString_Placeholder_ServiceName";
        private const String PlaceholderFilePath = "ConnectionString_Placeholder_FilePath";
        private const String PlaceholderInstanceName = "ConnectionString_Placeholder_InstanceName";

        private const String LabelHost = "ConnectionInfo_Label_Host";
        private const String LabelPort = "ConnectionInfo_Label_Port";
        private const String LabelInstance = "ConnectionInfo_Label_Instance";
        private const String LabelDatabase = "ConnectionInfo_Label_Database";
        private const String LabelServiceName = "ConnectionInfo_Label_ServiceName";
        private const String LabelUser = "ConnectionInfo_Label_User";
        private const String LabelPassword = "ConnectionInfo_Label_Password";
        private const String LabelFilePath = "ConnectionInfo_Label_FilePath";

        /// <summary>
        /// 各数据库类型的标准端口。端口为空时据此补默认值；
        /// SQLite 没有端口，不在表内。
        /// </summary>
        private static readonly Dictionary<String, Int32> DefaultPorts = new Dictionary<String, Int32>
        {
            { DbProviderConfig.SqlServer, 1433 },
            { DbProviderConfig.MySql, 3306 },
            { DbProviderConfig.Oracle, 1521 },
            { DbProviderConfig.PostgreSQL, 5432 }
        };

        /// <summary>
        /// 拼装连接参数清单。固定输出密码（为空时写占位符）。
        /// <paramref name="databaseName"/> 是调用方确认的库名（数据库节点取节点自身），
        /// 为空时回退到连接配置里的库名；仍为空则不输出「数据库」行。
        /// Provider 未知时返回 null；其余情况返回清单（可能含占位符）。
        /// </summary>
        public static DbConnectionInfoResult Build(
            DbViewModel model,
            String databaseName,
            ConnectionStringPlaceholderResolver textResolver)
        {
            if (model == null)
            {
                return null;
            }

            var provider = NormalizeProvider(model.Provider);
            if (provider == null)
            {
                return null;
            }

            var builder = new InfoBuilder(textResolver);

            // 端口为空时补该数据库类型的标准端口；默认值只是陈述事实，不计入待填写项。
            var port = model.Port > 0 ? model.Port : GetDefaultPort(provider);

            switch (provider)
            {
                case DbProviderConfig.SqlServer:
                    {
                        // DataSource 可能是 host\instance，也可能带 ",port"；
                        // 主机与具名实例分行呈现，避免把整串当成主机误导阅读方。
                        var dataSource = (model.Server ?? String.Empty).Trim();
                        var commaIndex = dataSource.IndexOf(',');
                        if (commaIndex >= 0)
                        {
                            dataSource = dataSource.Substring(0, commaIndex).Trim();
                        }

                        var slashIndex = dataSource.IndexOf('\\');
                        var host = slashIndex >= 0 ? dataSource.Substring(0, slashIndex).Trim() : dataSource;
                        var instance = slashIndex >= 0 ? dataSource.Substring(slashIndex + 1).Trim() : String.Empty;

                        // 实例也可能来自连接配置里单独填写的字段；只有两者都为空才视为没有实例。
                        if (instance.Length == 0 && !String.IsNullOrWhiteSpace(model.ServerInstance))
                        {
                            instance = model.ServerInstance.Trim();
                        }

                        AddServerFields(builder, host, port);

                        // 确实没有实例名时不输出该行。
                        if (instance.Length > 0)
                        {
                            builder.AddValue(LabelInstance, instance, PlaceholderInstanceName);
                        }

                        builder.AddValue(LabelDatabase, ResolveDatabaseName(databaseName, model), null);
                    }

                    AddCredentialFields(builder, model);
                    break;

                case DbProviderConfig.MySql:
                    AddServerFields(builder, model.Server, port);
                    builder.AddValue(LabelDatabase, ResolveDatabaseName(databaseName, model), null);
                    AddCredentialFields(builder, model);
                    break;

                case DbProviderConfig.PostgreSQL:
                    AddServerFields(builder, model.Server, port);
                    builder.AddValue(LabelDatabase, ResolveDatabaseName(databaseName, model), null);
                    AddCredentialFields(builder, model);
                    break;

                case DbProviderConfig.Oracle:
                    AddServerFields(builder, model.Server, port);
                    builder.AddValue(LabelServiceName, model.Instance, PlaceholderServiceName);
                    AddCredentialFields(builder, model);
                    break;

                case DbProviderConfig.SQLite:
                    // SQLite 没有主机/端口/用户名，库标识就是文件路径；
                    // 用「文件路径」标签而不是把路径塞进「主机」，避免误导。
                    builder.AddValue(LabelFilePath, model.Instance, PlaceholderFilePath);
                    builder.AddValue(LabelPassword, model.Password, PlaceholderPassword);
                    break;
            }

            return new DbConnectionInfoResult(builder.ToString(), builder.PlaceholderCount);
        }

        /// <summary>
        /// 规范化 Provider 名称；未知返回 null。
        /// </summary>
        public static String NormalizeProvider(String provider)
        {
            if (String.IsNullOrWhiteSpace(provider))
            {
                return null;
            }

            var value = provider.Trim();

            if (String.Equals(value, DbProviderConfig.SqlServer, StringComparison.OrdinalIgnoreCase)) return DbProviderConfig.SqlServer;
            if (String.Equals(value, DbProviderConfig.MySql, StringComparison.OrdinalIgnoreCase)) return DbProviderConfig.MySql;
            if (String.Equals(value, DbProviderConfig.Oracle, StringComparison.OrdinalIgnoreCase)) return DbProviderConfig.Oracle;
            if (String.Equals(value, DbProviderConfig.SQLite, StringComparison.OrdinalIgnoreCase)) return DbProviderConfig.SQLite;
            if (String.Equals(value, DbProviderConfig.PostgreSQL, StringComparison.OrdinalIgnoreCase)) return DbProviderConfig.PostgreSQL;

            return null;
        }

        /// <summary>
        /// 取该数据库类型的标准默认端口；无端口语义的类型（SQLite）返回 0。
        /// </summary>
        private static Int32 GetDefaultPort(String provider)
        {
            Int32 port;
            return DefaultPorts.TryGetValue(provider, out port) ? port : 0;
        }

        /// <summary>
        /// 主机 + 端口。端口在调用方已补齐默认值，这里只做呈现。
        /// </summary>
        private static void AddServerFields(InfoBuilder builder, String host, Int32 port)
        {
            builder.AddValue(LabelHost, host, PlaceholderHost);
            builder.AddValue(LabelPort, port.ToString(), null);
        }

        private static void AddCredentialFields(InfoBuilder builder, DbViewModel model)
        {
            builder.AddValue(LabelUser, model.User, PlaceholderUser);
            builder.AddValue(LabelPassword, model.Password, PlaceholderPassword);
        }

        /// <summary>
        /// 库名优先取调用方确认的值（数据库节点自身），否则回退到连接配置里的库名；
        /// Oracle 的 Instance 是服务名、SQLite 的 Instance 是文件路径，这两个分支不调用本方法。
        /// </summary>
        private static String ResolveDatabaseName(String databaseName, DbViewModel model)
        {
            if (!String.IsNullOrWhiteSpace(databaseName))
            {
                return databaseName;
            }

            return model == null ? null : model.Instance;
        }

        /// <summary>
        /// 逐行拼装器：一行一个 <c>名称: 值</c>，不对齐列宽，避免被 Markdown 当成表格。
        /// </summary>
        private sealed class InfoBuilder
        {
            private readonly ConnectionStringPlaceholderResolver _resolver;
            private readonly List<String> _lines = new List<String>();
            private Int32 _placeholderCount;

            public InfoBuilder(ConnectionStringPlaceholderResolver resolver)
            {
                _resolver = resolver;
            }

            public Int32 PlaceholderCount
            {
                get { return _placeholderCount; }
            }

            /// <summary>
            /// 追加一行。值为空时写成 <c>[占位符]</c> 并计入待填写项；
            /// 没有占位符语义的字段（如已补默认值的端口）传 null 即跳过空值行。
            /// </summary>
            public void AddValue(String labelKey, String value, String placeholderKey)
            {
                String text;

                if (String.IsNullOrWhiteSpace(value))
                {
                    if (placeholderKey == null)
                    {
                        return;
                    }

                    text = "[" + Resolve(placeholderKey) + "]";
                    _placeholderCount++;
                }
                else
                {
                    text = value.Trim();
                }

                _lines.Add(Resolve(labelKey) + ": " + text);
            }

            public override String ToString()
            {
                var builder = new StringBuilder();
                for (var i = 0; i < _lines.Count; i++)
                {
                    if (i > 0)
                    {
                        builder.Append("\r\n");
                    }

                    builder.Append(_lines[i]);
                }

                return builder.ToString();
            }

            private String Resolve(String key)
            {
                var value = _resolver == null ? null : _resolver(key);
                return String.IsNullOrEmpty(value) ? key : value;
            }
        }
    }
}
