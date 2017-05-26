using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.DbSchema.Provider
{
    public class SqlServerSchema : DbSchemaBase
    {
        public override List<Database> GetDatabases()
        {
            throw new NotImplementedException();
        }

        public override List<Table> GetTables(Database db)
        {
            throw new NotImplementedException();
        }

        public override List<Column> GetColumns(Table table)
        {
            throw new NotImplementedException();
        }
    }
}
