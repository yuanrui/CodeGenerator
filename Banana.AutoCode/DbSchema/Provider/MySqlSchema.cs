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

            return result;
        }

        public override List<Table> GetTables(Database db)
        {
            const string sql = @"
select table_name as Id, table_name as Name, table_comment as Comment, table_schema as Owner
from information_schema.tables
where table_schema=@TableSchema";

            var result = Context.Query<Table>(sql, new { TableSchema = db.Name });

            return result;
        }

        public override List<Column> GetColumns(Table table)
        {
            const string sql = @"
select ordinal_position as Id, column_name as Name, data_type as RawType, column_comment as Comment
from information_schema.columns where table_schema=@TableSchema
and table_name=@TableName";
            
            var result = Context.Query<Column>(sql, new { TableSchema = table.Owner, TableName = table.Name });

            return result;
        }

        public override Type GetType(string rawType, short precision, short scale, bool isNullable)
        {
            return typeof(Object);
        }

        public override DbType GetDbType(string rawType, short precision, short scale)
        {
            return DbType.Object;
        }
    }
}
