using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Banana.AutoCode.Core;

namespace Banana.AutoCode.DbSchema.Provider
{
    public class SqlServerSchema : DbSchemaBase
    {
        public SqlServerSchema(string connName) : base(connName)
        { 
            
        }

        public override List<Database> GetDatabases()
        {
            const string sql = "select name as DbName from sys.databases order by database_id desc";
           
            return Context.Query<Database>(sql);
        }

        public override List<Table> GetTables(Database db)
        {
            var sql = string.Format(@"USE [{0}];
select a.object_id as Id, a.name as Name, b.value as Comment from sys.tables a 
left join sys.extended_properties b on a.object_id = b.major_id and minor_id =0;", db.DbName);
            var result = Context.Query<Table>(sql);
            
            foreach (var item in result)
	        {
                item.DbName = db.DbName;
	        }

            return result;
        }

        public override List<Column> GetColumns(Table table)
        {
            var sql = string.Format(@"USE [{0}];
WITH colConsCTE(TABLE_NAME, COLUMN_NAME, CONSTRAINT_TYPE) AS (
     SELECT A.TABLE_NAME, A.COLUMN_NAME, B.CONSTRAINT_TYPE FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE A
     LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS B ON A.CONSTRAINT_NAME = B.CONSTRAINT_NAME AND A.TABLE_NAME = B.TABLE_NAME 
)
SELECT a.object_id AS TableID, a.name AS TableName, b.column_id AS Id, b.name as Name, d.value AS Comment , c.name AS DBType, b.max_length AS 'Length',
	CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE b.name = cte.COLUMN_NAME AND a.name = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='PRIMARY KEY') > 0 THEN 1 ELSE 0 END AS IsPrimaryKey,
	CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE b.name = cte.COLUMN_NAME AND a.name = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='FOREIGN KEY') > 0 THEN 1 ELSE 0 END AS IsForeignKey,
	b.IS_NULLABLE as 'IsNullAble'
FROM sys.tables a 
LEFT JOIN sys.columns b on a.object_id = b.object_id
LEFT JOIN sys.types c on b.user_type_id = c.user_type_id
LEFT JOIN sys.extended_properties d on a.object_id = d.major_id and d.minor_id = b.column_id
where a.name = @TableName ", table.DbName);

            var result = Context.Query<Column>(sql, new { TableName = table.Name });

            foreach (var item in result)
            {
                item.Table = table;
            }

            return result;
        }

    }
}
