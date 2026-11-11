//------------------------------------------------------------------------------
// <copyright file="DbConnectionStringFormats.cs">
//    Copyright (c) 2025, https://github.com/yuanrui All rights reserved.
// </copyright>
// <author>Yuan Rui</author>
// <date>2026-11-11 10:00:00</date>
//------------------------------------------------------------------------------

using Banana.AutoCode.DbSchema;
using System;
using System.IO;
using System.Text;

namespace Banana.AutoCode.Core
{
    /// <summary>
    /// 连接字符串格式标识，顺序与右键菜单一致。
    /// </summary>
    public enum DbConnectionFormat
    {
        AdoNet = 0,
        Jdbc = 1,
        OdbcDsnLess = 2,
        OleDb = 3,
        SqlAlchemyUrl = 4,
        NodeJsUrl = 5,
        GoDsn = 6,
        PhpPdoDsn = 7,
        RustDatabaseUrl = 8
    }

    /// <summary>
    /// 占位符文案解析器：入参为本地化键，返回当前 UI 语言下的提示词。
    /// </summary>
    public delegate String ConnectionStringPlaceholderResolver(String key);

    /// <summary>
    /// 拼装结果：文本与待填写项个数。
    /// </summary>
    public sealed class DbConnectionStringResult
    {
        private readonly String _text;
        private readonly Int32 _placeholderCount;

        public DbConnectionStringResult(String text, Int32 placeholderCount)
        {
            _text = text ?? String.Empty;
            _placeholderCount = placeholderCount;
        }

        /// <summary>
        /// 拼装出的连接字符串。
        /// </summary>
        public String Text
        {
            get { return _text; }
        }

        /// <summary>
        /// 文本中待填写项（<c>[占位符]</c>）的个数。
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
    /// 把 <see cref="DbViewModel"/> 拼装成各生态/协议的连接字符串。
    ///
    /// 三种状态：
    /// 1. 完整：字段齐全，输出真实值；
    /// 2. 缺字段：仍输出语法正确的字符串，缺失段落写成 <c>[占位符]</c>，个数见
    ///    <see cref="DbConnectionStringResult.PlaceholderCount"/>；
    /// 3. 语义不适用（如 Go DSN 对 SQL Server）：返回 <c>null</c>，菜单中不可用。
    ///
    /// 其余约束：纯函数，不读写配置、不依赖驱动包；占位符本身不做百分号编码。
    /// </summary>
    public static class DbConnectionStringFormats
    {
        public const String SqlServer = "Sql Server";
        public const String MySql = "MySql";
        public const String Oracle = "Oracle";
        public const String SQLite = "SQLite";
        public const String PostgreSQL = "PostgreSQL";

        public const String PlaceholderHost = "ConnectionString_Placeholder_Host";
        public const String PlaceholderDatabase = "ConnectionString_Placeholder_Database";
        public const String PlaceholderServiceName = "ConnectionString_Placeholder_ServiceName";
        public const String PlaceholderUser = "ConnectionString_Placeholder_User";
        public const String PlaceholderInstanceName = "ConnectionString_Placeholder_InstanceName";
        public const String PlaceholderFilePath = "ConnectionString_Placeholder_FilePath";

        private const Int32 DefaultSqlServerPort = 1433;
        private const Int32 DefaultMySqlPort = 3306;
        private const Int32 DefaultOraclePort = 1521;
        private const Int32 DefaultPostgreSqlPort = 5432;

        /// <summary>
        /// 所有格式，顺序与右键菜单一致。
        /// </summary>
        public static DbConnectionFormat[] AllFormats
        {
            get
            {
                return new[]
                {
                    DbConnectionFormat.AdoNet,
                    DbConnectionFormat.Jdbc,
                    DbConnectionFormat.OdbcDsnLess,
                    DbConnectionFormat.OleDb,
                    DbConnectionFormat.SqlAlchemyUrl,
                    DbConnectionFormat.NodeJsUrl,
                    DbConnectionFormat.GoDsn,
                    DbConnectionFormat.PhpPdoDsn,
                    DbConnectionFormat.RustDatabaseUrl
                };
            }
        }

        /// <summary>
        /// 菜单与文档使用的格式名（语言中性，不做本地化）。
        /// </summary>
        public static String GetFormatName(DbConnectionFormat format)
        {
            switch (format)
            {
                case DbConnectionFormat.AdoNet: return "ADO.NET";
                case DbConnectionFormat.Jdbc: return "JDBC URL";
                case DbConnectionFormat.OdbcDsnLess: return "ODBC (DSN-less)";
                case DbConnectionFormat.OleDb: return "OLE DB";
                case DbConnectionFormat.SqlAlchemyUrl: return "SQLAlchemy URL";
                case DbConnectionFormat.NodeJsUrl: return "Node.js URL";
                case DbConnectionFormat.GoDsn: return "Go DSN";
                case DbConnectionFormat.PhpPdoDsn: return "PHP PDO DSN";
                case DbConnectionFormat.RustDatabaseUrl: return "Rust DATABASE_URL";
                default: return format.ToString();
            }
        }

        /// <summary>
        /// 规范化 Provider 名称；未知返回 null。取值与 <c>DbProviderConfig</c> 中的常量一致。
        /// </summary>
        public static String NormalizeProvider(String provider)
        {
            if (String.IsNullOrWhiteSpace(provider))
            {
                return null;
            }

            var value = provider.Trim();

            if (String.Equals(value, SqlServer, StringComparison.OrdinalIgnoreCase)) return SqlServer;
            if (String.Equals(value, MySql, StringComparison.OrdinalIgnoreCase)) return MySql;
            if (String.Equals(value, Oracle, StringComparison.OrdinalIgnoreCase)) return Oracle;
            if (String.Equals(value, SQLite, StringComparison.OrdinalIgnoreCase)) return SQLite;
            if (String.Equals(value, PostgreSQL, StringComparison.OrdinalIgnoreCase)) return PostgreSQL;

            return null;
        }

        /// <summary>
        /// 各数据库类型的默认端口；SQLite 返回 0。
        /// </summary>
        public static Int32 GetDefaultPort(String provider)
        {
            var normalized = NormalizeProvider(provider);
            if (normalized == null)
            {
                return 0;
            }

            switch (normalized)
            {
                case SqlServer: return DefaultSqlServerPort;
                case MySql: return DefaultMySqlPort;
                case Oracle: return DefaultOraclePort;
                case PostgreSQL: return DefaultPostgreSqlPort;
                default: return 0;
            }
        }

        /// <summary>
        /// 该格式是否适用于该数据库类型。语义不适用返回 false，字段缺失不影响该判断。
        /// </summary>
        public static Boolean IsSupported(DbConnectionFormat format, String provider)
        {
            var normalized = NormalizeProvider(provider);
            if (normalized == null)
            {
                return false;
            }

            switch (format)
            {
                case DbConnectionFormat.AdoNet:
                case DbConnectionFormat.Jdbc:
                case DbConnectionFormat.OdbcDsnLess:
                case DbConnectionFormat.PhpPdoDsn:
                    return true;

                case DbConnectionFormat.OleDb:
                    // 只有 SQL Server / Oracle 有稳定的 OLE DB Provider。
                    return normalized == SqlServer || normalized == Oracle;

                case DbConnectionFormat.SqlAlchemyUrl:
                    // SQLAlchemy 的 Oracle 方言需要额外驱动名，本功能不生成。
                    return normalized == SqlServer || normalized == MySql
                        || normalized == SQLite || normalized == PostgreSQL;

                case DbConnectionFormat.NodeJsUrl:
                    // 任务约定的 Node.js 支持范围：postgres / mysql / mssql。
                    return normalized == SqlServer || normalized == MySql || normalized == PostgreSQL;

                case DbConnectionFormat.GoDsn:
                    // go-sql-driver/mysql 与 pgx/lib/pq；SQL Server/Oracle/SQLite 无对应 Go DSN。
                    return normalized == MySql || normalized == PostgreSQL;

                case DbConnectionFormat.RustDatabaseUrl:
                    // sqlx 的 sqlite URL 斜杠规则与 sqlalchemy 不同，本功能不生成。
                    return normalized == SqlServer || normalized == MySql || normalized == PostgreSQL;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 拼装指定格式的连接字符串。
        /// 语义不适用或 Provider 未知时返回 null；字段缺失时返回带占位符的结果。
        /// </summary>
        /// <param name="format">目标格式。</param>
        /// <param name="model">连接模型。</param>
        /// <param name="placeholderResolver">占位符文案解析器，按当前 UI 语言返回提示词。</param>
        public static DbConnectionStringResult Build(
            DbConnectionFormat format,
            DbViewModel model,
            ConnectionStringPlaceholderResolver placeholderResolver)
        {
            if (model == null)
            {
                return null;
            }

            var provider = NormalizeProvider(model.Provider);
            if (provider == null || !IsSupported(format, provider))
            {
                return null;
            }

            var template = new Template(placeholderResolver);

            switch (provider)
            {
                case SqlServer:
                    BuildSqlServer(format, model, template);
                    break;
                case MySql:
                    BuildMySql(format, model, template);
                    break;
                case Oracle:
                    BuildOracle(format, model, template);
                    break;
                case SQLite:
                    BuildSQLite(format, model, template);
                    break;
                case PostgreSQL:
                    BuildPostgreSql(format, model, template);
                    break;
            }

            return new DbConnectionStringResult(template.ToString(), template.PlaceholderCount);
        }

        #region SQL Server

        private static void BuildSqlServer(DbConnectionFormat format, DbViewModel model, Template template)
        {
            if (format == DbConnectionFormat.AdoNet)
            {
                template.Raw("Data Source=");
                AppendSqlServerAdoDataSource(template, model);
                template.Raw(";Initial Catalog=").QuotedOrPlaceholder(model.Instance, PlaceholderDatabase)
                    .Raw(";User ID=").QuotedOrPlaceholder(model.User, PlaceholderUser)
                    .Raw(";Password=").Raw(AdoQuote(model.Password)).Raw(";");
                return;
            }

            if (format == DbConnectionFormat.Jdbc)
            {
                template.Raw("jdbc:sqlserver://");
                if (IsEmpty(model.Server))
                {
                    template.Placeholder(PlaceholderHost);
                }
                else
                {
                    template.Raw(EncodeUrlComponent(SqlServerHostName(model.Server)));
                }

                template.Raw(":").Raw(EffectivePort(model).ToString());

                if (!IsEmpty(model.Server))
                {
                    var server = model.Server.Trim();
                    var index = server.IndexOf('\\');
                    if (index >= 0)
                    {
                        template.Raw(";instanceName=");
                        var instance = server.Substring(index + 1);
                        if (IsEmpty(instance))
                        {
                            template.Placeholder(PlaceholderInstanceName);
                        }
                        else
                        {
                            template.Raw(instance);
                        }
                    }
                }

                template.Raw(";databaseName=").EncodedOrPlaceholder(model.Instance, PlaceholderDatabase);
                return;
            }

            if (format == DbConnectionFormat.OdbcDsnLess)
            {
                template.Raw("Driver={ODBC Driver 17 for SQL Server};Server=");
                AppendSqlServerBracedDataSource(template, model);
                template.Raw(";Database=").OdbcOrPlaceholder(model.Instance, PlaceholderDatabase)
                    .Raw(";UID=").OdbcOrPlaceholder(model.User, PlaceholderUser)
                    .Raw(";PWD=").Raw(OdbcValue(model.Password)).Raw(";");
                return;
            }

            if (format == DbConnectionFormat.OleDb)
            {
                template.Raw("Provider=SQLOLEDB;Data Source=");
                AppendSqlServerBracedDataSource(template, model);
                template.Raw(";Initial Catalog=").OdbcOrPlaceholder(model.Instance, PlaceholderDatabase)
                    .Raw(";User ID=").OdbcOrPlaceholder(model.User, PlaceholderUser)
                    .Raw(";Password=").Raw(OdbcValue(model.Password)).Raw(";");
                return;
            }

            if (format == DbConnectionFormat.SqlAlchemyUrl)
            {
                template.Raw("mssql+pyodbc://");
                AppendUrlUserInfo(template, model);
                template.Raw("@");
                AppendSqlServerUrlHost(template, model);
                template.Raw(":").Raw(EffectivePort(model).ToString())
                    .Raw("/").EncodedOrPlaceholder(model.Instance, PlaceholderDatabase)
                    .Raw("?driver=ODBC+Driver+17+for+SQL+Server");
                return;
            }

            if (format == DbConnectionFormat.NodeJsUrl || format == DbConnectionFormat.RustDatabaseUrl)
            {
                template.Raw("mssql://");
                AppendUrlUserInfo(template, model);
                template.Raw("@");
                AppendSqlServerUrlHost(template, model);
                template.Raw(":").Raw(EffectivePort(model).ToString())
                    .Raw("/").EncodedOrPlaceholder(model.Instance, PlaceholderDatabase);
                return;
            }

            if (format == DbConnectionFormat.PhpPdoDsn)
            {
                template.Raw("sqlsrv:Server=");
                if (IsEmpty(model.Server))
                {
                    template.Placeholder(PlaceholderHost);
                }
                else
                {
                    var server = model.Server.Trim();
                    var index = server.IndexOf('\\');
                    if (index < 0)
                    {
                        template.Raw(server).Raw(",").Raw(EffectivePort(model).ToString());
                    }
                    else
                    {
                        template.Raw(server.Substring(0, index + 1));
                        var instance = server.Substring(index + 1);
                        if (IsEmpty(instance))
                        {
                            template.Placeholder(PlaceholderInstanceName);
                        }
                        else
                        {
                            template.Raw(instance);
                        }
                    }
                }

                template.Raw(";Database=").RawOrPlaceholder(model.Instance, PlaceholderDatabase);
            }
        }

        /// <summary>
        /// ADO.NET 的 Data Source：缺主机用占位符；具名实例不带端口，其余按需追加非默认端口。
        /// </summary>
        private static void AppendSqlServerAdoDataSource(Template template, DbViewModel model)
        {
            if (IsEmpty(model.Server))
            {
                template.Placeholder(PlaceholderHost);
                return;
            }

            var server = model.Server.Trim();
            var index = server.IndexOf('\\');

            if (index >= 0)
            {
                template.Raw(server.Substring(0, index + 1));
                var instance = server.Substring(index + 1);
                if (IsEmpty(instance))
                {
                    template.Placeholder(PlaceholderInstanceName);
                }
                else
                {
                    template.Raw(instance);
                }

                return;
            }

            template.Raw(server);
            if (model.Port > 0 && model.Port != DefaultSqlServerPort)
            {
                template.Raw(",").Raw(model.Port.ToString());
            }
        }

        /// <summary>
        /// ODBC/OLE DB 的 Data Source：缺主机用占位符，其余总是显式写端口。
        /// </summary>
        private static void AppendSqlServerBracedDataSource(Template template, DbViewModel model)
        {
            if (IsEmpty(model.Server))
            {
                template.Raw("{");
                template.Placeholder(PlaceholderHost);
                template.Raw("}");
                return;
            }

            var server = model.Server.Trim();
            var index = server.IndexOf('\\');
            if (index >= 0)
            {
                template.Raw("{" + server.Substring(0, index + 1));
                var instance = server.Substring(index + 1);
                if (IsEmpty(instance))
                {
                    template.Placeholder(PlaceholderInstanceName);
                }
                else
                {
                    template.Raw(EscapeBraces(instance));
                }

                template.Raw("}");
                return;
            }

            template.OdbcOrPlaceholder(server + "," + EffectivePort(model), PlaceholderHost);
        }

        /// <summary>
        /// 去掉具名实例部分，只保留主机名。
        /// </summary>
        private static String SqlServerHostName(String server)
        {
            var value = (server ?? String.Empty).Trim();
            var index = value.IndexOf('\\');
            return index >= 0 ? value.Substring(0, index) : value;
        }

        /// <summary>
        /// URL 风格的 SQL Server 主机段：具名实例写成 host%5Cinstance，实例名缺失时用占位符。
        /// </summary>
        private static void AppendSqlServerUrlHost(Template template, DbViewModel model)
        {
            if (IsEmpty(model.Server))
            {
                template.Placeholder(PlaceholderHost);
                return;
            }

            var server = model.Server.Trim();
            var index = server.IndexOf('\\');

            if (index < 0)
            {
                template.Raw(EncodeUrlComponent(server));
                return;
            }

            template.Raw(EncodeUrlComponent(server.Substring(0, index))).Raw("%5C");

            var instance = server.Substring(index + 1);
            if (IsEmpty(instance))
            {
                template.Placeholder(PlaceholderInstanceName);
            }
            else
            {
                template.Raw(EncodeUrlComponent(instance));
            }
        }

        #endregion

        #region MySQL

        private static void BuildMySql(DbConnectionFormat format, DbViewModel model, Template template)
        {
            if (format == DbConnectionFormat.AdoNet)
            {
                template.Raw("Server=").QuotedOrPlaceholder(model.Server, PlaceholderHost)
                    .Raw(";Port=").Raw(EffectivePort(model).ToString())
                    .Raw(";Database=").QuotedOrPlaceholder(model.Instance, PlaceholderDatabase)
                    .Raw(";User ID=").QuotedOrPlaceholder(model.User, PlaceholderUser)
                    .Raw(";Password=").Raw(AdoQuote(model.Password))
                    .Raw(";SslMode=None;AllowPublicKeyRetrieval=True;AllowUserVariables=True;CharacterSet=utf8mb4;");
                return;
            }

            if (format == DbConnectionFormat.Jdbc)
            {
                template.Raw("jdbc:mysql://").RawOrPlaceholder(model.Server, PlaceholderHost)
                    .Raw(":").Raw(EffectivePort(model).ToString())
                    .Raw("/").EncodedOrPlaceholder(model.Instance, PlaceholderDatabase);
                return;
            }

            if (format == DbConnectionFormat.OdbcDsnLess)
            {
                template.Raw("Driver={MySQL ODBC 8.0 Unicode Driver};Server=").OdbcOrPlaceholder(model.Server, PlaceholderHost)
                    .Raw(";Port=").Raw(EffectivePort(model).ToString())
                    .Raw(";Database=").OdbcOrPlaceholder(model.Instance, PlaceholderDatabase)
                    .Raw(";UID=").OdbcOrPlaceholder(model.User, PlaceholderUser)
                    .Raw(";PWD=").Raw(OdbcValue(model.Password)).Raw(";");
                return;
            }

            if (format == DbConnectionFormat.SqlAlchemyUrl || format == DbConnectionFormat.NodeJsUrl
                || format == DbConnectionFormat.RustDatabaseUrl)
            {
                if (format == DbConnectionFormat.SqlAlchemyUrl)
                {
                    template.Raw("mysql+pymysql://");
                }
                else
                {
                    template.Raw("mysql://");
                }

                AppendUrlUserInfo(template, model);
                template.Raw("@").RawOrPlaceholder(model.Server, PlaceholderHost)
                    .Raw(":").Raw(EffectivePort(model).ToString())
                    .Raw("/").EncodedOrPlaceholder(model.Instance, PlaceholderDatabase);
                return;
            }

            if (format == DbConnectionFormat.GoDsn)
            {
                // go-sql-driver/mysql 的 DSN 不是 URL，用户名与密码原样插入，不做百分号编码。
                template.RawOrPlaceholder(model.User, PlaceholderUser)
                    .Raw(":").Raw(model.Password ?? String.Empty)
                    .Raw("@tcp(").RawOrPlaceholder(model.Server, PlaceholderHost)
                    .Raw(":").Raw(EffectivePort(model).ToString())
                    .Raw(")/").RawOrPlaceholder(model.Instance, PlaceholderDatabase)
                    .Raw("?charset=utf8mb4&parseTime=True&loc=Local");
                return;
            }

            if (format == DbConnectionFormat.PhpPdoDsn)
            {
                template.Raw("mysql:host=").RawOrPlaceholder(model.Server, PlaceholderHost)
                    .Raw(";port=").Raw(EffectivePort(model).ToString())
                    .Raw(";dbname=").RawOrPlaceholder(model.Instance, PlaceholderDatabase)
                    .Raw(";charset=utf8mb4");
            }
        }

        #endregion

        #region Oracle

        private static void BuildOracle(DbConnectionFormat format, DbViewModel model, Template template)
        {
            if (format == DbConnectionFormat.AdoNet)
            {
                template.Raw("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=")
                    .RawOrPlaceholder(model.Server, PlaceholderHost)
                    .Raw(")(PORT=").Raw(EffectivePort(model).ToString())
                    .Raw("))(CONNECT_DATA=(SERVICE_NAME=")
                    .RawOrPlaceholder(model.Instance, PlaceholderServiceName)
                    .Raw(")));User ID=").QuotedOrPlaceholder(model.User, PlaceholderUser)
                    .Raw(";Password=").Raw(AdoQuote(model.Password)).Raw(";");
                return;
            }

            if (format == DbConnectionFormat.Jdbc)
            {
                // 连接模型保存的是 SERVICE_NAME；缺失时用占位符，SID 用户可按文档改写为 SID 语法。
                template.Raw("jdbc:oracle:thin:@//").RawOrPlaceholder(model.Server, PlaceholderHost)
                    .Raw(":").Raw(EffectivePort(model).ToString())
                    .Raw("/").RawOrPlaceholder(model.Instance, PlaceholderServiceName);
                return;
            }

            if (format == DbConnectionFormat.OdbcDsnLess)
            {
                template.Raw("Driver={Oracle in OraClient19Home1};DBQ={");
                if (IsEmpty(model.Server))
                {
                    template.Placeholder(PlaceholderHost);
                }
                else
                {
                    template.Raw(EscapeBraces(model.Server));
                }

                template.Raw(":").Raw(EffectivePort(model).ToString()).Raw("/");
                if (IsEmpty(model.Instance))
                {
                    template.Placeholder(PlaceholderServiceName);
                }
                else
                {
                    template.Raw(EscapeBraces(model.Instance));
                }

                template.Raw("};UID=").OdbcOrPlaceholder(model.User, PlaceholderUser)
                    .Raw(";PWD=").Raw(OdbcValue(model.Password)).Raw(";");
                return;
            }

            if (format == DbConnectionFormat.OleDb)
            {
                template.Raw("Provider=OraOLEDB.Oracle;Data Source={");
                if (IsEmpty(model.Server))
                {
                    template.Placeholder(PlaceholderHost);
                }
                else
                {
                    template.Raw(EscapeBraces(model.Server));
                }

                template.Raw(":").Raw(EffectivePort(model).ToString()).Raw("/");
                if (IsEmpty(model.Instance))
                {
                    template.Placeholder(PlaceholderServiceName);
                }
                else
                {
                    template.Raw(EscapeBraces(model.Instance));
                }

                template.Raw("};User ID=").OdbcOrPlaceholder(model.User, PlaceholderUser)
                    .Raw(";Password=").Raw(OdbcValue(model.Password)).Raw(";");
                return;
            }

            if (format == DbConnectionFormat.PhpPdoDsn)
            {
                template.Raw("oci:dbname=").RawOrPlaceholder(model.Server, PlaceholderHost)
                    .Raw(":").Raw(EffectivePort(model).ToString())
                    .Raw("/").RawOrPlaceholder(model.Instance, PlaceholderServiceName)
                    .Raw(";charset=AL32UTF8");
            }
        }

        #endregion

        #region SQLite

        private static void BuildSQLite(DbConnectionFormat format, DbViewModel model, Template template)
        {
            if (format == DbConnectionFormat.AdoNet)
            {
                template.Raw("Data Source=").QuotedOrPlaceholder(model.Instance, PlaceholderFilePath).Raw(";");
                if (!IsEmpty(model.Password))
                {
                    template.Raw("Password=").Raw(AdoQuote(model.Password)).Raw(";");
                }

                return;
            }

            if (format == DbConnectionFormat.Jdbc)
            {
                template.Raw("jdbc:sqlite:").RawOrPlaceholder(NormalizeSeparators(model.Instance), PlaceholderFilePath);
                return;
            }

            if (format == DbConnectionFormat.OdbcDsnLess)
            {
                template.Raw("Driver={SQLite3 ODBC Driver};Database=")
                    .OdbcOrPlaceholder(model.Instance, PlaceholderFilePath).Raw(";");
                return;
            }

            if (format == DbConnectionFormat.SqlAlchemyUrl)
            {
                // 相对路径 3 个斜杠，绝对路径 4 个斜杠；占位符不编码。
                if (String.IsNullOrWhiteSpace(model.Instance))
                {
                    template.Raw("sqlite:///").Placeholder(PlaceholderFilePath);
                    return;
                }

                var path = NormalizeSeparators(model.Instance);
                if (IsAbsolutePath(model.Instance))
                {
                    template.Raw("sqlite:////").Raw(path.TrimStart('/'));
                }
                else
                {
                    template.Raw("sqlite:///").Raw(TrimRelativePrefix(path));
                }

                return;
            }

            if (format == DbConnectionFormat.PhpPdoDsn)
            {
                template.Raw("sqlite:").RawOrPlaceholder(NormalizeSeparators(model.Instance), PlaceholderFilePath);
            }
        }

        #endregion

        #region PostgreSQL

        private static void BuildPostgreSql(DbConnectionFormat format, DbViewModel model, Template template)
        {
            if (format == DbConnectionFormat.AdoNet)
            {
                template.Raw("Host=").QuotedOrPlaceholder(model.Server, PlaceholderHost)
                    .Raw(";Port=").Raw(EffectivePort(model).ToString())
                    .Raw(";Database=").QuotedOrPlaceholder(model.Instance, PlaceholderDatabase)
                    .Raw(";Username=").QuotedOrPlaceholder(model.User, PlaceholderUser)
                    .Raw(";Password=").Raw(AdoQuote(model.Password)).Raw(";");
                return;
            }

            if (format == DbConnectionFormat.Jdbc)
            {
                template.Raw("jdbc:postgresql://").RawOrPlaceholder(model.Server, PlaceholderHost)
                    .Raw(":").Raw(EffectivePort(model).ToString())
                    .Raw("/").EncodedOrPlaceholder(model.Instance, PlaceholderDatabase);
                return;
            }

            if (format == DbConnectionFormat.OdbcDsnLess)
            {
                template.Raw("Driver={PostgreSQL Unicode};Server=").OdbcOrPlaceholder(model.Server, PlaceholderHost)
                    .Raw(";Port=").Raw(EffectivePort(model).ToString())
                    .Raw(";Database=").OdbcOrPlaceholder(model.Instance, PlaceholderDatabase)
                    .Raw(";UID=").OdbcOrPlaceholder(model.User, PlaceholderUser)
                    .Raw(";PWD=").Raw(OdbcValue(model.Password)).Raw(";");
                return;
            }

            if (format == DbConnectionFormat.SqlAlchemyUrl)
            {
                template.Raw("postgresql+psycopg2://");
                AppendUrlUserInfo(template, model);
                template.Raw("@").RawOrPlaceholder(model.Server, PlaceholderHost)
                    .Raw(":").Raw(EffectivePort(model).ToString())
                    .Raw("/").EncodedOrPlaceholder(model.Instance, PlaceholderDatabase);
                return;
            }

            if (format == DbConnectionFormat.NodeJsUrl || format == DbConnectionFormat.GoDsn
                || format == DbConnectionFormat.RustDatabaseUrl)
            {
                template.Raw("postgres://");
                AppendUrlUserInfo(template, model);
                template.Raw("@").RawOrPlaceholder(model.Server, PlaceholderHost)
                    .Raw(":").Raw(EffectivePort(model).ToString())
                    .Raw("/").EncodedOrPlaceholder(model.Instance, PlaceholderDatabase)
                    .Raw("?sslmode=disable");
                return;
            }

            if (format == DbConnectionFormat.PhpPdoDsn)
            {
                template.Raw("pgsql:host=").RawOrPlaceholder(model.Server, PlaceholderHost)
                    .Raw(";port=").Raw(EffectivePort(model).ToString())
                    .Raw(";dbname=").RawOrPlaceholder(model.Instance, PlaceholderDatabase);
            }
        }

        #endregion

        #region 公共格式化

        private static Int32 EffectivePort(DbViewModel model)
        {
            return model.Port > 0 ? model.Port : GetDefaultPort(model.Provider);
        }

        private static Boolean IsEmpty(String value)
        {
            return String.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// URL 的 userinfo 段：用户名缺失用占位符，密码为空时省略冒号。
        /// </summary>
        private static void AppendUrlUserInfo(Template template, DbViewModel model)
        {
            template.EncodedOrPlaceholder(model.User, PlaceholderUser);

            if (!IsEmpty(model.Password))
            {
                template.Raw(":").Raw(EncodeUrlComponent(model.Password));
            }
        }

        /// <summary>
        /// RFC 3986 百分号编码：保留 A-Z a-z 0-9 - . _ ~，其余按 UTF-8 逐字节编码。
        /// </summary>
        public static String EncodeUrlComponent(String value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }

            var builder = new StringBuilder(value.Length * 3);

            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];

                if (IsUrlUnreserved(c))
                {
                    builder.Append(c);
                    continue;
                }

                if (Char.IsHighSurrogate(c) && i + 1 < value.Length && Char.IsLowSurrogate(value[i + 1]))
                {
                    AppendPercentEncoded(builder, Encoding.UTF8.GetBytes(value.Substring(i, 2)));
                    i++;
                    continue;
                }

                AppendPercentEncoded(builder, Encoding.UTF8.GetBytes(c.ToString()));
            }

            return builder.ToString();
        }

        private static Boolean IsUrlUnreserved(Char c)
        {
            return (c >= 'A' && c <= 'Z')
                || (c >= 'a' && c <= 'z')
                || (c >= '0' && c <= '9')
                || c == '-' || c == '.' || c == '_' || c == '~';
        }

        private static void AppendPercentEncoded(StringBuilder builder, Byte[] bytes)
        {
            foreach (var b in bytes)
            {
                builder.Append('%').Append(b.ToString("X2"));
            }
        }

        /// <summary>
        /// ADO.NET 键值对引号规则：值含 ; = 引号或首尾空白时加引号，
        /// 内部同种引号成对转义。
        /// </summary>
        private static String AdoQuote(String value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }

            var needsQuote = value.IndexOf(';') >= 0
                || value.IndexOf('=') >= 0
                || value.IndexOf('"') >= 0
                || value.IndexOf('\'') >= 0
                || value.Length != value.Trim().Length;

            if (!needsQuote)
            {
                return value;
            }

            if (value.IndexOf('"') >= 0 && value.IndexOf('\'') < 0)
            {
                return "'" + value.Replace("'", "''") + "'";
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private static String EscapeBraces(String value)
        {
            return (value ?? String.Empty).Replace("}", "}}");
        }

        /// <summary>
        /// ODBC / OLE DB 值：统一用大括号包裹，内部右大括号转义为两个。
        /// </summary>
        private static String OdbcValue(String value)
        {
            return "{" + EscapeBraces(value) + "}";
        }

        private static String NormalizeSeparators(String path)
        {
            return (path ?? String.Empty).Replace('\\', '/');
        }

        private static String TrimRelativePrefix(String path)
        {
            var value = path.TrimStart('/');
            while (value.StartsWith("./", StringComparison.Ordinal))
            {
                value = value.Substring(2);
            }

            return value;
        }

        private static Boolean IsAbsolutePath(String path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            return Path.IsPathRooted(path);
        }

        #endregion

        /// <summary>
        /// 拼装缓冲：统一负责占位符计数与不编码规则。
        /// </summary>
        private sealed class Template
        {
            private readonly StringBuilder _buffer = new StringBuilder();
            private readonly ConnectionStringPlaceholderResolver _resolver;
            private Int32 _placeholderCount;

            internal Template(ConnectionStringPlaceholderResolver resolver)
            {
                _resolver = resolver;
            }

            internal Int32 PlaceholderCount
            {
                get { return _placeholderCount; }
            }

            internal Template Raw(String value)
            {
                _buffer.Append(value);
                return this;
            }

            internal Template Placeholder(String key)
            {
                _placeholderCount++;
                var text = _resolver == null ? key : _resolver(key);
                if (String.IsNullOrEmpty(text))
                {
                    text = key;
                }

                _buffer.Append('[').Append(text).Append(']');
                return this;
            }

            /// <summary>
            /// 原样输出；空值用占位符。
            /// </summary>
            internal Template RawOrPlaceholder(String value, String placeholderKey)
            {
                if (IsEmpty(value))
                {
                    return Placeholder(placeholderKey);
                }

                return Raw(value);
            }

            /// <summary>
            /// 百分号编码后输出；空值用占位符（占位符不编码）。
            /// </summary>
            internal Template EncodedOrPlaceholder(String value, String placeholderKey)
            {
                if (IsEmpty(value))
                {
                    return Placeholder(placeholderKey);
                }

                return Raw(EncodeUrlComponent(value));
            }

            /// <summary>
            /// ADO.NET 引号规则后输出；空值用占位符。
            /// </summary>
            internal Template QuotedOrPlaceholder(String value, String placeholderKey)
            {
                if (IsEmpty(value))
                {
                    return Placeholder(placeholderKey);
                }

                return Raw(AdoQuote(value));
            }

            /// <summary>
            /// 大括号包裹后输出；空值用大括号包裹的占位符（替换发生在括号内）。
            /// </summary>
            internal Template OdbcOrPlaceholder(String value, String placeholderKey)
            {
                if (IsEmpty(value))
                {
                    Raw("{");
                    Placeholder(placeholderKey);
                    return Raw("}");
                }

                return Raw(OdbcValue(value));
            }

            public override String ToString()
            {
                return _buffer.ToString();
            }
        }
    }
}
