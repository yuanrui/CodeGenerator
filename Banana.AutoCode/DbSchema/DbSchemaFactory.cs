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
            var dbSchemaBase = DbProviderConfig.CreateDbSchemaBase(connSetting);
            if (dbSchemaBase == null)
            {
                return new SqlServerSchema(connSetting.Name);
            }

            return dbSchemaBase;
        }
    }
}
