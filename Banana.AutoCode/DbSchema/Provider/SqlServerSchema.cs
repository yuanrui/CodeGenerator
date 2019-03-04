using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Banana.AutoCode.Core;

namespace Banana.AutoCode.DbSchema.Provider
{
    public class SqlServerSchema : DbSchemaBase
    {
        public SqlServerSchema(String connName) : base(connName)
        { 
            
        }

        public override List<Database> GetDatabases()
        {
            const String sql = "select name from sys.databases order by database_id desc";
           
            return Context.Query<Database>(sql);
        }

        public override List<Table> GetTables(Database db)
        {
            var sql = String.Format(@"USE [{0}];
select a.object_id as Id, a.name as Name, b.value as Comment from sys.tables a 
left join sys.extended_properties b on a.object_id = b.major_id and minor_id =0;", db.Name);
            var result = Context.Query<Table>(sql);
            
            return result;
        }

        public override List<Column> GetColumns(Table table)
        {
            var sql = String.Format(@"USE [{0}];
WITH colConsCTE(TABLE_NAME, COLUMN_NAME, CONSTRAINT_TYPE) AS (
     SELECT A.TABLE_NAME, A.COLUMN_NAME, B.CONSTRAINT_TYPE FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE A
     LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS B ON A.CONSTRAINT_NAME = B.CONSTRAINT_NAME AND A.TABLE_NAME = B.TABLE_NAME 
)
SELECT a.object_id AS TableId, a.name AS TableName, b.column_id AS Id, b.name as Name, d.value AS Comment , c.name AS RawType,
	CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE b.name = cte.COLUMN_NAME AND a.name = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='PRIMARY KEY') > 0 THEN 1 ELSE 0 END AS IsPrimaryKey,
	CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE b.name = cte.COLUMN_NAME AND a.name = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='FOREIGN KEY') > 0 THEN 1 ELSE 0 END AS IsForeignKey,
    CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE b.name = cte.COLUMN_NAME AND a.name = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='UNIQUE') > 0 THEN 1 ELSE 0 END AS IsUnique,
	b.IS_NULLABLE as 'IsNullable',
    b.max_length AS 'Length',
	c.precision as 'Precision',
	c.scale as 'Scale'
FROM sys.tables a 
LEFT JOIN sys.columns b on a.object_id = b.object_id
LEFT JOIN sys.types c on b.user_type_id = c.user_type_id
LEFT JOIN sys.extended_properties d on a.object_id = d.major_id and d.minor_id = b.column_id
where a.name = @TableName ", table.Owner);
            
            var cmd = Context.DatabaseObject.GetSqlStringCommand(sql);
            Context.DatabaseObject.AddInParameter(cmd, "TableName", DbType.String, table.Name);

            var result = new List<Column>();

            using (var reader = Context.ExecuteReader(cmd))
            {
                while (reader.Read())
                {
                    var column = Fill(reader);
                    result.Add(column);
                }
            }

            return result;
        }


        public override Type GetType(String rawType, Int16 precision, Int16 scale, bool isNullable)
        {
            if (String.IsNullOrEmpty(rawType))
            {
                throw new ArgumentException("The rawType is null or empty.", "rawType");
            }

            switch (rawType.ToLowerInvariant())
            {
                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                case "time":
                    return GetTypeOf<DateTime>(isNullable);
                case "bit":
                case "boolean":
                    return GetTypeOf<Boolean>(isNullable);
                case "tinyint":
                    return GetTypeOf<Byte>(isNullable);
                case "smallint":
                    return GetTypeOf<Int16>(isNullable);
                case "int":
                case "integer":
                    return GetTypeOf<Int32>(isNullable);
                case "bigint":
                case "long":
                    return GetTypeOf<Int64>(isNullable);
                case "decimal":
                case "smallmoney":
                case "money":
                case "numeric":
                    return GetTypeOf<Decimal>(isNullable);
                case "real":
                    return GetTypeOf<Single>(isNullable);
                case "float":
                    return GetTypeOf<Double>(isNullable);
                case "char":
                case "nchar":
                case "varchar":
                case "nvarchar":
                case "text":
                case "ntext":
                    return typeof(String);
                case "blob":
                case "binary":
                case "image":
                case "rowversion":
                case "timestamp":
                case "varbinary":
                    return typeof(byte[]);
                case "uniqueidentifier":
                    return GetTypeOf<Guid>(isNullable);
                case "xml":
                    return typeof(String);
                default:
                    return rawType.Contains("int") ? GetTypeOf<Int32>(isNullable) : typeof(String);
            }
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/cc716729.aspx
        /// </summary>
        /// <param name="rawType"></param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public override DbType GetDbType(String rawType, Int16 precision, Int16 scale)
        {
            if (String.IsNullOrEmpty(rawType))
            {
                throw new ArgumentException("The rawType is null or empty.", "rawType");
            }

            switch (rawType.ToLowerInvariant())
            {
                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    return DbType.DateTime;
                case "time":
                    return DbType.Time;
                case "bit":
                case "boolean":
                    return DbType.Boolean;
                case "tinyint":
                    return DbType.Byte;
                case "smallint":
                    return DbType.Int16;
                case "int":
                case "integer":
                    return DbType.Int32;
                case "bigint":
                case "long":
                    return DbType.Int64;
                case "decimal":
                case "smallmoney":
                case "money":
                case "numeric":
                    return DbType.Decimal;
                case "real":
                    return DbType.Single;
                case "float":
                    return DbType.Double;
                case "char":
                    return DbType.AnsiStringFixedLength;
                case "varchar":
                case "text":
                    return DbType.AnsiString;
                case "nchar":
                case "nvarchar":
                case "ntext":
                    return DbType.String;
                case "blob":
                case "binary":
                case "image":
                case "rowversion":
                case "timestamp":
                case "varbinary":
                    return DbType.Binary;
                case "uniqueidentifier":
                    return DbType.Guid;
                case "xml":
                    return DbType.Xml;
                default:
                    return DbType.Object;
            }
        }
    }
}
