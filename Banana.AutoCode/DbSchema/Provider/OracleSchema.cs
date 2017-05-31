using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.DbSchema.Provider
{
    public class OracleSchema : DbSchemaBase
    {
        public OracleSchema(string connName) : base(connName)
        { 
            
        }

        public override List<Database> GetDatabases()
        {
            const string sql = "select USERNAME as Name from dba_users";

            return Context.Query<Database>(sql);
        }

        public override List<Table> GetTables(Database db)
        {
            const string sql = @"select o.OBJECT_ID as Id, t.TABLE_NAME as Name, c.COMMENTS as ""Comment"", t.OWNER as Owner
from dba_tables t 
left join dba_tab_comments c on t.TABLE_NAME = c.TABLE_NAME 
left join ALL_OBJECTS o on t.TABLE_NAME = o.OBJECT_NAME and t.OWNER = o.OWNER
where t.owner=:Owner";

            var result = Context.Query<Table>(sql, new { Owner = db.Name });

            return result;
        }

        public override List<Column> GetColumns(Table table)
        {
            const string sql = @"
WITH colConsCTE AS (
    SELECT a.*, b.CONSTRAINT_TYPE FROM user_cons_columns a
    LEFT JOIN user_constraints b ON a.constraint_name = b.constraint_name AND a.table_name = b.table_name 
    WHERE a.TABLE_NAME = :TableName 
)

SELECT tab.COLUMN_ID AS Id, tab.TABLE_NAME AS TableName, tab.COLUMN_NAME AS Name, col.COMMENTS AS ""Comment""
  , tab.DATA_TYPE AS DbType
  , tab.DATA_LENGTH AS Length
  , tab.DATA_PRECISION AS Precision
  , tab.DATA_SCALE AS Scale
  , CASE tab.NULLABLE WHEN 'Y' THEN 1 ELSE 0 END AS IsNullable
  , CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE tab.COLUMN_NAME = cte.COLUMN_NAME AND tab.TABLE_NAME = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='P') > 0 THEN 1 ELSE 0 END AS IsPrimaryKey
  , CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE tab.COLUMN_NAME = cte.COLUMN_NAME AND tab.TABLE_NAME = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='R') > 0 THEN 1 ELSE 0 END AS IsForeignKey
  , CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE tab.COLUMN_NAME = cte.COLUMN_NAME AND tab.TABLE_NAME = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='U') > 0 THEN 1 ELSE 0 END AS IsUnique
  , CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE tab.COLUMN_NAME = cte.COLUMN_NAME AND tab.TABLE_NAME = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='O') > 0 THEN 1 ELSE 0 END AS IsReadOnly
  , 0 AS IsIdentity
FROM USER_TAB_COLUMNS tab
LEFT JOIN USER_COL_COMMENTS col ON tab.table_name = col.table_name AND tab.COLUMN_NAME = col.COLUMN_NAME
WHERE tab.table_name = :TableName 
ORDER BY tab.COLUMN_ID ASC ";

            var result = Context.Query<Column>(sql, new { TableName = table.Name });

            return result;
        }
    }
}
