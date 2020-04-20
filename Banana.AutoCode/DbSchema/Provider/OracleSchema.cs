using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.DbSchema.Provider
{
    public class OracleSchema : DbSchemaBase
    {
        public OracleSchema(String connName) : base(connName)
        { 
            
        }

        public override List<Database> GetDatabases()
        {
            const String sql = "select USERNAME as Name from all_users";

            return Context.Query<Database>(sql);
        }

        public override List<Table> GetTables(Database db)
        {
            const String sql = @"select 
o.OBJECT_ID as Id, 
t.TABLE_NAME as Name, c.COMMENTS as ""Comment"", t.OWNER as Owner
from all_tables t 
left join all_tab_comments c on t.owner = c.owner and t.TABLE_NAME = c.TABLE_NAME 
left join ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME 
where t.owner=:Owner
order by t.TABLE_NAME";

            var result = Context.Query<Table>(sql, new { Owner = db.Name });

            return result;
        }

        public override List<Column> GetColumns(Table table)
        {
            const String sql = @"
WITH colConsCTE AS (
    SELECT a.*, b.CONSTRAINT_TYPE FROM all_cons_columns a
    LEFT JOIN all_constraints b ON a.owner = b.owner AND a.constraint_name = b.constraint_name AND a.table_name = b.table_name 
    WHERE a.owner = :Owner and a.TABLE_NAME = :TableName 
)

SELECT tab.COLUMN_ID AS Id, tab.TABLE_NAME AS TableName, tab.COLUMN_NAME AS Name, col.COMMENTS AS ""Comment""
  , tab.DATA_TYPE AS RawType
  , tab.DATA_LENGTH AS Length
  , tab.DATA_PRECISION AS Precision
  , tab.DATA_SCALE AS Scale
  , CASE tab.NULLABLE WHEN 'Y' THEN 1 ELSE 0 END AS IsNullable
  , CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE tab.COLUMN_NAME = cte.COLUMN_NAME AND tab.TABLE_NAME = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='P') > 0 THEN 1 ELSE 0 END AS IsPrimaryKey
  , CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE tab.COLUMN_NAME = cte.COLUMN_NAME AND tab.TABLE_NAME = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='R') > 0 THEN 1 ELSE 0 END AS IsForeignKey
  , CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE tab.COLUMN_NAME = cte.COLUMN_NAME AND tab.TABLE_NAME = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='U') > 0 THEN 1 ELSE 0 END AS IsUnique
  , CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE tab.COLUMN_NAME = cte.COLUMN_NAME AND tab.TABLE_NAME = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='O') > 0 THEN 1 ELSE 0 END AS IsReadOnly
  , 0 AS IsIdentity
FROM ALL_TAB_COLUMNS tab
LEFT JOIN ALL_COL_COMMENTS col ON tab.owner = col.owner AND tab.table_name = col.table_name AND tab.COLUMN_NAME = col.COLUMN_NAME
WHERE tab.owner = :Owner AND tab.table_name = :TableName 
ORDER BY tab.COLUMN_ID ASC ";
            var result = new List<Column>();
            var cmd = Context.DatabaseObject.GetSqlStringCommand(sql);
            Context.DatabaseObject.AddInParameter(cmd, "Owner", DbType.String, table.Owner);
            Context.DatabaseObject.AddInParameter(cmd, "TableName", DbType.String, table.Name);

            using (var reader = Context.ExecuteReader(cmd))
            {
                while (reader.Read())
                {
                    var column = Fill(reader);

                    result.Add(column);
                }
            }

            //var result = Context.Query<Column>(sql, new { TableName = table.Name });

            return result;
        }

        protected override void FixRawType(Column column)
        {
            if (column.RawType.StartsWith("TIMESTAMP", StringComparison.OrdinalIgnoreCase))
            {
                column.RawType = "TIMESTAMP";
                return;
            }

            base.FixRawType(column);
        }

        /// <summary>
        /// convert Oracle number type
        /// http://docs.oracle.com/cd/E51173_01/win.122/e17732/entityDataTypeMapping.htm
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <param name="isNullable"></param>
        /// <returns></returns>
        private static Type ConvertNumberToType(Int16 precision, Int16 scale, Boolean isNullable)
        {
            if (scale == 0)
            {
                if (precision == 0)
                {
                    return GetTypeOf<Int64>(isNullable);
                }

                if (precision == 1)
                {
                    return GetTypeOf<Boolean>(isNullable);
                }

                if (precision <= 3)
                {
                    return GetTypeOf<Byte>(isNullable);
                }

                if (precision <= 4)
                {
                    return GetTypeOf<Int16>(isNullable);
                }

                if (precision <= 10)
                {
                    return GetTypeOf<Int32>(isNullable);
                }

                if (precision <= 19)
                {
                    return GetTypeOf<Int64>(isNullable);
                }
            }

            return GetTypeOf<Decimal>(isNullable);
        }

        public override Type GetType(String rawType, Int16 precision, Int16 scale, Boolean isNullable)
        {
            if (String.IsNullOrEmpty(rawType))
            {
                throw new ArgumentException("The rawType is null or empty.", "rawType");
            }

            switch (rawType.ToUpper())
            {
                case "BFILE": 
                case "BLOB":
                case "RAW":
                case "LONG RAW":
                    return typeof(Byte[]);
                case "UNSIGNED INTEGER":
                case "FLOAT": 
                    return GetTypeOf<Decimal>(isNullable);
                case "INTEGER":
                    return GetTypeOf<Int64>(isNullable);
                case "INTERVAL YEAR TO MONTH":
                    return GetTypeOf<Int32>(isNullable);
                case "NUMBER": 
                    return ConvertNumberToType(precision, scale, isNullable);
                case "INTERVAL DAY TO SECOND":
                    return GetTypeOf<TimeSpan>(isNullable);
                case "DATE": 
                case "TIMESTAMP": 
                case "TIMESTAMP WITH LOCAL TIME ZONE": 
                case "TIMESTAMP WITH TIME ZONE":
                    return GetTypeOf<DateTime>(isNullable);
                case "CHAR":
                case "CLOB":
                case "LONG":
                case "NCHAR":
                case "NCLOB":
                case "ROWID":
                case "NVARCHAR2":
                case "VARCHAR2": 
                    return typeof(String);
                default: 
                    return typeof(Object);
            }
        }

        /// <summary>
        /// http://docs.oracle.com/html/B14164_01/featOraCommand.htm
        /// https://msdn.microsoft.com/en-us/library/yk72thhd%28v=vs.80%29.aspx
        /// </summary>
        /// <param name="rawType"></param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public override DbType GetDbType(String rawType, Int16 precision, Int16 scale)
        {
            var csharpDbType = DbType.Object;
            switch (rawType.ToUpper())
            {
                case "BFILE": csharpDbType = DbType.Object; break;
                case "BLOB": csharpDbType = DbType.Object; break;
                case "CHAR": csharpDbType = DbType.AnsiStringFixedLength; break;
                case "CLOB": csharpDbType = DbType.Object; break;
                case "DATE": csharpDbType = DbType.DateTime; break;
                case "FLOAT": csharpDbType = DbType.Decimal; break;
                case "INTEGER": csharpDbType = DbType.Int64; break;
                case "INTERVAL YEAR TO MONTH": csharpDbType = DbType.Int32; break;
                case "INTERVAL DAY TO SECOND": csharpDbType = DbType.Object; break;
                case "LONG": csharpDbType = DbType.AnsiString; break;
                case "LONG RAW": csharpDbType = DbType.Binary; break;
                case "NCHAR": csharpDbType = DbType.StringFixedLength; break;
                case "NCLOB": csharpDbType = DbType.Object; break;
                case "NUMBER": csharpDbType = DbType.Decimal; break;
                case "NVARCHAR2": csharpDbType = DbType.String; break;
                case "RAW": csharpDbType = DbType.Binary; break;
                case "ROWID": csharpDbType = DbType.AnsiString; break;
                case "TIMESTAMP": csharpDbType = DbType.DateTime; break;
                case "TIMESTAMP WITH LOCAL TIME ZONE": csharpDbType = DbType.DateTime; break;
                case "TIMESTAMP WITH TIME ZONE": csharpDbType = DbType.DateTime; break;
                case "UNSIGNED INTEGER": csharpDbType = DbType.Decimal; break;
                case "VARCHAR2": csharpDbType = DbType.AnsiString; break;
                default: csharpDbType = DbType.Object; break;
            }

            return csharpDbType;
        }
    }
}
