using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.DbSchema.Provider
{
    public class MySqlSchema : DbSchemaBase
    {
        public MySqlSchema(string connName) : base(connName)
        {

        }

        public override List<Database> GetDatabases()
        {
            var result = new List<Database>();
            var cmd = Context.DbProviderFactory.CreateCommand();
            cmd.CommandText = "show databases";

            using (var reader = Context.ExecuteReader(cmd))
            {
                while (reader.Read())
                {
                    var dbName = reader.GetString(0);

                    result.Add(new Database() { Name = dbName });
                }
            }

            return result.OrderBy(m => m.Name).ToList() ?? new List<Database>();
        }

        public override List<Table> GetTables(Database db)
        {
            const string sql = @"
select table_name as Id, table_name as Name, table_comment as Comment, table_schema as Owner
from information_schema.tables
where table_schema=@TableSchema
order by table_name";

            var result = Context.Query<Table>(sql, new { TableSchema = db.Name });

            return result ?? new List<Table>();
        }

        public override List<Column> GetColumns(Table table)
        {
            const string sql = @"
select col.ordinal_position as Id, col.column_name as Name, data_type as RawType, column_type as RawType2, column_comment as Comment
, case when 
(
select count(*) from information_schema.key_column_usage usg, information_schema.table_constraints cons 
where usg.table_schema = @TableSchema and usg.table_name = @TableName 
and cons.table_schema = @TableSchema and cons.table_name = @TableName 
and col.column_name = usg.column_name and usg.constraint_name = cons.constraint_name
and cons.constraint_type = 'PRIMARY KEY'
) > 0 then 1 else 0 end as IsPrimaryKey
, case when 
(
select count(*) from information_schema.key_column_usage usg, information_schema.table_constraints cons 
where usg.table_schema = @TableSchema and usg.table_name = @TableName 
and cons.table_schema = @TableSchema and cons.table_name = @TableName 
and col.column_name = usg.column_name and usg.constraint_name = cons.constraint_name
and cons.constraint_type = 'FOREIGN KEY'
) > 0 then 1 else 0 end as IsForeignKey
, case when 
(
select count(*) from information_schema.key_column_usage usg, information_schema.table_constraints cons 
where usg.table_schema = @TableSchema and usg.table_name = @TableName 
and cons.table_schema = @TableSchema and cons.table_name = @TableName 
and col.column_name = usg.column_name and usg.constraint_name = cons.constraint_name
and cons.constraint_type = 'UNIQUE'
) > 0 then 1 else 0 end as IsUnique
, case is_nullable when 'YES' then 1 else 0 end IsNullable
, character_maximum_length as Length, numeric_precision as ""Precision"", numeric_scale as Scale
from information_schema.columns col
where col.table_schema = @TableSchema and col.table_name = @TableName
order by 1";
            
            var result = new List<Column>();
            var cmd = Context.DatabaseObject.GetSqlStringCommand(sql);
            Context.DatabaseObject.AddInParameter(cmd, "TableSchema", DbType.String, table.Owner);
            Context.DatabaseObject.AddInParameter(cmd, "TableName", DbType.String, table.Name);

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

        protected override void FixRawType(Column column)
        {
            if (column.RawType2 == "tinyint(1)" || column.RawType2 == "tinyint(1) unsigned")
            {
                column.Precision = 1;
                return;
            }

            base.FixRawType(column);
        }

        public override Type GetType(string rawType, short precision, short scale, bool isNullable)
        {
            if (String.IsNullOrEmpty(rawType))
            {
                throw new ArgumentException("The rawType is null or empty.", "rawType");
            }
            
            switch (rawType.ToLower())
            {
                case "bit":
                case "bool":
                case "boolean":
                    return GetTypeOf<Boolean>(isNullable);
                case "tinyint":
                    return ConvertToNumberType(precision, scale, isNullable);
                case "smallint":
                    return GetTypeOf<Int16>(isNullable);
                case "int":
                case "integer":
                case "mediumint":
                    return GetTypeOf<Int32>(isNullable);
                case "bigint":
                    return GetTypeOf<Int64>(isNullable);
                case "float":
                    return GetTypeOf<Single>(isNullable);
                case "double":
                    return GetTypeOf<Double>(isNullable);
                case "numeric":
                case "decimal":
                case "dec":
                case "real":
                    return GetTypeOf<Decimal>(isNullable);
                case "date":
                case "year":
                case "time":
                case "timestamp":
                case "datetime":
                    return GetTypeOf<DateTime>(isNullable);
                case "char":
                case "varchar":
                case "text":
                case "tinytext":
                case "longtext":
                    return typeof(String);
                case "binary":
                case "varbinary":
                case "blob":
                case "mediumblob":
                case "longblob":
                    return typeof(Byte[]);
                default:
                    return typeof(Object);
            }
        }

        public override DbType GetDbType(string rawType, short precision, short scale)
        {
            switch (rawType.ToLower())
            {
                case "bit":
                case "bool":
                case "boolean":
                    return DbType.Boolean;
                case "tinyint":
                    return DbType.Byte;
                case "smallint":                
                    return DbType.Int16;
                case "int":
                case "integer":
                case "mediumint":
                    return DbType.Int32;
                case "bigint":
                    return DbType.Int64;
                case "float":
                    return DbType.Single;
                case "double":
                    return DbType.Double;
                case "numeric":
                case "decimal":
                case "dec":
                case "real":
                    return DbType.Decimal;
                case "year":
                case "date":
                case "time":
                case "timestamp":
                case "datetime":
                    return DbType.DateTime;
                case "char":
                case "varchar":
                case "text":
                case "tinytext":
                case "longtext":
                    return DbType.String;
                case "binary":
                case "varbinary":
                case "blob":
                case "mediumblob":
                case "longblob":
                    return DbType.Binary;
                default:
                    return DbType.Object;
            }
        }
    }
}
