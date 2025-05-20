//------------------------------------------------------------------------------
// <copyright file="DbViewExtension.cs">
//    Copyright (c) 2025, https://github.com/yuanrui All rights reserved.
// </copyright>
// <author>Yuan Rui</author>
// <date>2025-05-08 18:00:00</date>
//------------------------------------------------------------------------------

#if NET
using MySqlConnector;
#else
using MySql.Data.MySqlClient;
#endif
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace Banana.AutoCode.DbSchema
{
    public static class DbViewExtension
    {
        private static string GetMatchText(string input, string pattern)
        {
            var match = Regex.Match(input, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }

        public static DbViewModel ToSqlServerModel(this ConnectionStringSettings connSetting)
        {
            var model = new DbViewModel();
            model.Name = connSetting.Name;
            model.Provider = DbProviderConfig.SqlServer;

            var builder = new SqlConnectionStringBuilder(connSetting.ConnectionString);
            model.Server = builder.DataSource;
            model.User = builder.UserID;
            model.Password = builder.Password;
            model.Instance = builder.InitialCatalog;

            var sources = builder.DataSource.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (sources != null && sources.Length == 2)
            {
                var port = 1433;
                if (!int.TryParse(sources[1], out port))
                {
                    port = 1433;
                }

                model.Port = port;
            }

            return model;
        }

        public static DbViewModel ToMySqlModel(this ConnectionStringSettings connSetting)
        {
            var model = new DbViewModel();
            model.Name = connSetting.Name;
            model.Provider = DbProviderConfig.MySql;

            var builder = new MySqlConnectionStringBuilder(connSetting.ConnectionString);
            model.Server = builder.Server;
            model.Port = (int)builder.Port;
            model.User = builder.UserID;
            model.Password = builder.Password;
            model.Instance = builder.Database;

            return model;
        }

        public static DbViewModel ToSQLiteModel(this ConnectionStringSettings connSetting)
        {
            var model = new DbViewModel();
            model.Name = connSetting.Name;
            model.Provider = DbProviderConfig.SQLite;

            var builder = new SQLiteConnectionStringBuilder(connSetting.ConnectionString);
            model.Password = builder.Password;
            model.Instance = builder.DataSource;

            return model;
        }

        public static DbViewModel ToOracleModel(this ConnectionStringSettings connSetting)
        {
            var model = new DbViewModel();
            model.Name = connSetting.Name;
            model.Provider = DbProviderConfig.Oracle;

            var builder = new OracleConnectionStringBuilder(connSetting.ConnectionString);
            var dataSource = builder.DataSource;

            model.Server = GetMatchText(dataSource, @"\(HOST=?(.+?)\)");
            var port = 1521;
            var portText = GetMatchText(dataSource, @"\(PORT=?(.+?)\)");
            if (!int.TryParse(portText, out port))
            {
                port = 1521;
            }
            model.Port = port;
            model.Instance = GetMatchText(dataSource, @"\(SERVICE_NAME=?(.+?)\)");
            model.User = builder.UserID;
            model.Password = builder.Password;
            return model;
        }

        public static DbViewModel ToPostgreSqlModel(this ConnectionStringSettings connSetting)
        {
            var model = new DbViewModel();
            model.Name = connSetting.Name;
            model.Provider = DbProviderConfig.PostgreSQL;

            var builder = new NpgsqlConnectionStringBuilder(connSetting.ConnectionString);
            if (builder == null)
            {
                return model;
            }

            model.Server = builder.Host;
            model.Port = builder.Port;
            model.Instance = builder.Database;
#if NET
            model.User = builder.Username;
#else
            model.User = builder.UserName;
#endif
            model.Password = builder.Password;
            
            return model;
        }

        public static string ToSqlServerConnectionString(this DbViewModel model)
        {
            var builder = new SqlConnectionStringBuilder();
            builder.InitialCatalog = model.Instance;
            builder.DataSource = model.Server;
            builder.UserID = model.User;
            builder.Password = model.Password;

            if (model.Port != 0 && model.Port != 1433)
            {
                builder.DataSource += "," + model.Port;
            }

            return builder.ToString();
        }

        public static string ToMySqlConnectionString(this DbViewModel model)
        {
            if (model.Port == 0)
            {
                model.Port = 3306;
            }
            var builder = new MySqlConnectionStringBuilder();
            builder.Server = model.Server;
            builder.Port = (uint)model.Port;
            builder.Database = model.Instance;
            builder.UserID = model.User;
            builder.Password = model.Password;
            builder.SslMode = MySqlSslMode.None;
            builder.AllowPublicKeyRetrieval = true;
            builder.AllowUserVariables = true;
            builder.CharacterSet = "utf8mb4";

            return builder.ToString();
        }

        public static string ToOracleConnectionString(this DbViewModel model)
        {
            if (model.Port == 0)
            {
                model.Port = 1521;
            }

            var builder = new OracleConnectionStringBuilder();
            builder.DataSource = $"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={model.Server})(PORT={model.Port}))(CONNECT_DATA=(SERVICE_NAME={model.Instance})))";
            builder.UserID = model.User;
            builder.Password = model.Password;

            return builder.ToString();
        }

        public static string ToSQLiteConnectionString(this DbViewModel model)
        {
            var builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = model.Instance;
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                builder.Password = model.Password;
            }

            return builder.ToString();
        }

        public static string ToPostgreSqlConnectionString(this DbViewModel model)
        {
            if (model.Port == 0)
            {
                model.Port = 5432;
            }

            var builder = new NpgsqlConnectionStringBuilder();
            builder.Host = model.Server;
            builder.Port = model.Port;
            builder.Database = model.Instance;
            builder.Password = model.Password;
#if NET
            builder.Username = model.User;
            builder.NoResetOnClose = true;

#else
            builder.UserName = model.User;
#endif

            return builder.ToString();
        }
    }
}
