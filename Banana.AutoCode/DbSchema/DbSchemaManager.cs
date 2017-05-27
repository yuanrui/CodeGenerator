using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Banana.AutoCode.Core;

namespace Banana.AutoCode.DbSchema
{
    public class DbSchemaManager
    {
        protected DbSchemaBase Provider;

        protected string ConnectionName;

        public DbSchemaManager(ConnectionStringSettings connSetting)
        {
            ConnectionName = connSetting.Name;
            Provider = DbSchemaFactory.Create(connSetting);
        }

        public virtual List<Database> GetComplexDatabases()
        {
            using (var ctx = DataContextScope.GetCurrent(ConnectionName).Begin())
            {
                var dbs = Provider.GetDatabases();

                if (dbs != null)
                {
                    foreach (var db in dbs)
                    {
                        db.Tables = Provider.GetTables(db);
                    }
                }

                return dbs;
            }
        }
    }
}
