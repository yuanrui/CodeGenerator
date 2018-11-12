using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Banana.AutoCode.DbSchema.Provider;

namespace Banana.AutoCode.DbSchema
{
    public class DbSchemaFactory
    {
        public static DbSchemaBase Create(ConnectionStringSettings connSetting)
        {
            switch (connSetting.ProviderName)
            {
                case "System.Data.SqlClient":
                    return new SqlServerSchema(connSetting.Name);
                case "System.Data.SQLite":
                    return new SQLiteSchema(connSetting.Name);
                case "Oracle.ManagedDataAccess.Client":
                    return new OracleSchema(connSetting.Name);
                case "MySql.Data.MySqlClient":
                    return new MySqlSchema(connSetting.Name);
                default:
                    return new SqlServerSchema(connSetting.Name);
            }
        }
    }
}
