//------------------------------------------------------------------------------
// <copyright file="DbProviderConfig.cs">
//    Copyright (c) 2025, https://github.com/yuanrui All rights reserved.
// </copyright>
// <author>Yuan Rui</author>
// <date>2025-05-08 18:00:00</date>
//------------------------------------------------------------------------------

using Banana.AutoCode.DbSchema.Provider;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Banana.AutoCode.DbSchema
{
    public class DbProviderConfig
    {
        public const string SqlServer = "Sql Server";
        public const string MySql = "MySql";
        public const string Oracle = "Oracle";
        public const string SQLite = "SQLite";
        public const string PostgreSQL = "PostgreSQL";

        public class NS
        {
            public const string System_Data_SqlClient = "System.Data.SqlClient";
            public const string Microsoft_Data_SqlClient = "Microsoft.Data.SqlClient";
            public const string System_Data_SQLite = "System.Data.SQLite";
            public const string Oracle_ManagedDataAccess_Client = "Oracle.ManagedDataAccess.Client";
            public const string MySql_Data_MySqlClient = "MySql.Data.MySqlClient";
#if NET
            public const string MySqlConnector = "MySqlConnector";
#else
            public const string MySqlConnector = "MySql.Data.MySqlClient";
#endif
            public const string Npgsql = "Npgsql";
        }

        protected class ProviderFunc
        {
            public Func<ConnectionStringSettings, DbSchemaBase> CreateDbSchemaBase { get; set; }

            public Func<ConnectionStringSettings, DbViewModel> CreateViewModel {  get; set; }
        }

        protected class SettingFunc
        {
            public int Port { get; set; }

            public Func<DbViewModel, ConnectionStringSettings> ConnSettingFunc { get; set; }
        }

        protected static readonly Dictionary<string, ProviderFunc> _providerFactory = new Dictionary<string, ProviderFunc>
        {
            {
                NS.System_Data_SqlClient, new ProviderFunc
                {
                    CreateDbSchemaBase = setting => new SqlServerSchema(setting.Name),
                    CreateViewModel = setting => setting.ToSqlServerModel()
                }
            },
            {
                NS.Microsoft_Data_SqlClient, new ProviderFunc
                {
                    CreateDbSchemaBase = setting => new SqlServerSchema(setting.Name),
                    CreateViewModel = setting => setting.ToSqlServerModel()
                }
            },
            {
                NS.System_Data_SQLite, new ProviderFunc
                {
                    CreateDbSchemaBase = setting => new SQLiteSchema(setting.Name),
                    CreateViewModel = setting => setting.ToSQLiteModel()
                }
            },
            {
                NS.Oracle_ManagedDataAccess_Client, new ProviderFunc
                {
                    CreateDbSchemaBase = setting => new OracleSchema(setting.Name),
                    CreateViewModel = setting => setting.ToOracleModel()
                }
            },
            {
                NS.MySqlConnector, new ProviderFunc
                {
                    CreateDbSchemaBase = setting => new MySqlSchema(setting.Name),
                    CreateViewModel = setting => setting.ToMySqlModel()
                }
            },
            {
                NS.Npgsql, new ProviderFunc
                {
                    CreateDbSchemaBase = setting => new PostgreSqlSchema(setting.Name),
                    CreateViewModel = setting => setting.ToPostgreSqlModel()
                }
            }
        };

        protected static readonly Dictionary<string, SettingFunc> _settingFactory = new Dictionary<string, SettingFunc>
        {
            {
                SqlServer, new SettingFunc
                {
                    Port = 1433,
                    ConnSettingFunc = model =>
                    {
                        var settings = new ConnectionStringSettings();
                        settings.Name = model.Name;
                        settings.ConnectionString = model.ToSqlServerConnectionString();
                        settings.ProviderName = NS.System_Data_SqlClient;
                        return settings;
                    }
                }
            },
            {
                MySql, new SettingFunc
                {
                    Port = 3306,
                    ConnSettingFunc = model =>
                    {
                        var settings = new ConnectionStringSettings();
                        settings.Name = model.Name;
                        settings.ConnectionString = model.ToMySqlConnectionString();
                        settings.ProviderName = NS.MySqlConnector;
                        return settings;
                    }
                }
            },
            {
                Oracle, new SettingFunc
                {
                    Port = 1521,
                    ConnSettingFunc = model =>
                    {
                        var settings = new ConnectionStringSettings();
                        settings.Name = model.Name;
                        settings.ConnectionString = model.ToOracleConnectionString();
                        settings.ProviderName = NS.Oracle_ManagedDataAccess_Client;
                        return settings;
                    }
                }
            },
            {
                SQLite, new SettingFunc
                {
                    Port = 0,
                    ConnSettingFunc = model =>
                    {
                        var settings = new ConnectionStringSettings();
                        settings.Name = model.Name;
                        settings.ConnectionString = model.ToSQLiteConnectionString();
                        settings.ProviderName = NS.System_Data_SQLite;
                        return settings;
                    }
                }
            }
            ,
            {
                PostgreSQL, new SettingFunc
                {
                    Port = 5432,
                    ConnSettingFunc = model =>
                    {
                        var settings = new ConnectionStringSettings();
                        settings.Name = model.Name;
                        settings.ConnectionString = model.ToPostgreSqlConnectionString();
                        settings.ProviderName = NS.Npgsql;
                        return settings;
                    }
                }
            }
        };

        public static DbViewModel CreateDbViewModel(ConnectionStringSettings connSetting)
        {
            if (connSetting == null)
            {
                return null;
            }
            
            if (!_providerFactory.ContainsKey(connSetting.ProviderName))
            {
                return null;
            }

            return _providerFactory[connSetting.ProviderName].CreateViewModel(connSetting);
        }

        public static DbSchemaBase CreateDbSchemaBase(ConnectionStringSettings connSetting)
        {
            if (connSetting == null)
            {
                return null;
            }

            if (!_providerFactory.ContainsKey(connSetting.ProviderName))
            {
                return null;
            }

            return _providerFactory[connSetting.ProviderName].CreateDbSchemaBase(connSetting);
        }


        public static ConnectionStringSettings ToSettings(DbViewModel model)
        {
            if (model == null)
            {
                return null;
            }

            if (! _settingFactory.ContainsKey(model.Provider))
            {
                return null;
            }

            return _settingFactory[model.Provider].ConnSettingFunc(model);
        }

        public static int ToPort(DbViewModel model)
        {
            if (model == null)
            {
                return 0;
            }

            return ToPort(model.Provider);
        }

        public static int ToPort(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                return 0;
            }

            if (!_settingFactory.ContainsKey(providerName))
            {
                return 0;
            }

            return _settingFactory[providerName].Port;
        }

        public static string[] GetDataProviders()
        {
            return _settingFactory.Keys.ToArray();
        }
    }
}
