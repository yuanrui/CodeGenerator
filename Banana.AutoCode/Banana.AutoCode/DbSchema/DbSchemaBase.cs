using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.DbSchema
{
    public abstract class DbSchemaBase
    {
        public abstract List<Database> GetDatabases();

        public abstract List<Table> GetTables(Database db);

        public abstract List<Column> GetColumns(Table table);
    }
}
