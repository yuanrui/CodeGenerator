using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Banana.AutoCode.Core;

namespace Banana.AutoCode.DbSchema
{
    public abstract class DbSchemaBase
    {
        protected virtual string ConnectionName { get; set; }

        public virtual DataContext Context { get; set; }

        public abstract List<Database> GetDatabases();

        public abstract List<Table> GetTables(Database db);

        public abstract List<Column> GetColumns(Table table);

        public virtual List<T> Query<T>(string sql, dynamic param = null)
        {
            using (var ctx = DataContextScope.GetCurrent(ConnectionName).Begin())
            {
                return ctx.DataContext.Query<T>(sql);
            }
        }
        
        public DbSchemaBase(string connName)
        {
            ConnectionName = connName;

            Context = DataContextScope.GetCurrent(ConnectionName).DataContext;
        }

    }
}
