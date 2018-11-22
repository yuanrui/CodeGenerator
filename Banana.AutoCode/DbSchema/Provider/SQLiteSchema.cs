using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.DbSchema.Provider
{
    public class SQLiteSchema : DbSchemaBase
    {
        #region SQLite schema structure

        private class SQLiteColumnInfo 
        {
            public Int32 id { get; set; }

            public String name { get; set; }

            public String type { get; set; }

            public Boolean notnull { get; set; }

            public String dflt_value { get; set; }

            public Boolean pk { get; set; }
        }

        private class SQLiteForeignKey
        {
            public Int32 id { get; set; }

            public Int32 seq { get; set; }

            public String table { get; set; }

            public String from { get; set; }

            public String to { get; set; }

            public String on_update { get; set; }

            public String on_delete { get; set; }

            public String match { get; set; }
        }

        private class SQLiteIndex
        {
            public class Field 
            {
                public Int32 seqno { get; set; }

                public Int32 cid { get; set; }
                
                public String name { get; set; }
            }

            public Int32 seq { get; set; }

            public String name { get; set; }

            public Boolean unique { get; set; }

            public String origin { get; set; }

            public Int32 partial { get; set; }
        }

        #endregion

        public override string MetaDataCollectionName_Databases
        {
            get
            {
                return "Catalogs";
            }
        }

        public SQLiteSchema(string connName) : base(connName)
        { 
            
        }

        public override List<Database> GetDatabases()
        {
            return new List<Database> 
            { 
                new Database() 
                { 
                    Name = "main" 
                }
            };
        }

        public override List<Column> GetColumns(Table table)
        {
            var columns = Context.Query<SQLiteColumnInfo>("PRAGMA table_info(" + table.Name + ")") ?? new List<SQLiteColumnInfo>();
            var fks = Context.Query<SQLiteForeignKey>("PRAGMA foreign_key_list(" + table.Name + ")") ?? new List<SQLiteForeignKey>();
            var indexs = Context.Query<SQLiteIndex>("PRAGMA index_list(" + table.Name + ")") ?? new List<SQLiteIndex>();
            var result = base.GetColumns(table);

            foreach (var item in columns)
            {
                var col = result.FirstOrDefault(m => m.Name == item.name);
                if (col == null)
                {
                    continue;
                }

                col.IsPrimaryKey = item.pk;
                col.IsNullable = !item.notnull;
                col.RawType = item.type;
            }

            foreach (var fk in fks)
            {
                var col = result.FirstOrDefault(m => m.Name == fk.from);
                col.IsForeignKey = col != null;
            }

            foreach (var index in indexs)
            {
                if (! index.unique)
                {
                    continue;
                }

                var fields = Context.Query<SQLiteIndex.Field>("PRAGMA index_info(" + index.name + ")");

                if (fields == null || fields.Count != 1)
                {
                    continue;
                }

                foreach (var item in fields)
                {
                    var col = result.FirstOrDefault(m => m.Name == item.name);
                    col.IsUnique = col != null;
                }
            }

            return result;
        }

        private static Type ConvertNumberToType(Int16 precision, Int16 scale, Boolean isNullable)
        {
            if (scale <= 0)
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

                if (precision <= 9)
                {
                    return GetTypeOf<Int32>(isNullable);
                }

                if (precision <= 18)
                {
                    return GetTypeOf<Int64>(isNullable);
                }
            }

            return GetTypeOf<Decimal>(isNullable);
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
                    return GetTypeOf<Boolean>(isNullable);
                case "smallint":
                    return GetTypeOf<Int16>(isNullable);
                case "int":
                case "integer":
                    return GetTypeOf<Int32>(isNullable);
                case "float":
                    return GetTypeOf<Single>(isNullable);
                case "double":
                    return GetTypeOf<Double>(isNullable);
                case "decimal":
                case "real":
                    return GetTypeOf<Decimal>(isNullable);
                case "numeric":
                    return ConvertNumberToType(precision, scale, isNullable);
                case "date":
                case "time":
                case "timestamp":
                case "datetime":
                    return GetTypeOf<DateTime>(isNullable);
                case "char":
                case "varchar":
                case "graphic":
                case "vargraphic":
                case "nvarchar":
                case "text":
                    return typeof(String);
                case "blob":
                    return typeof(Byte[]);
                default:
                    return typeof(Object);
            }
        }

        public override DbType GetDbType(string rawType, short precision, short scale)
        {
            if (String.IsNullOrEmpty(rawType))
            {
                throw new ArgumentException("The rawType is null or empty.", "rawType");
            }

            switch (rawType.ToLower())
            {
                case "bit":
                    return DbType.Boolean;
                case "smallint":
                    return DbType.Int16;
                case "int":
                case "integer":
                    return DbType.Int32;
                case "float":
                    return DbType.Single;
                case "double":
                    return DbType.Double;
                case "decimal":
                case "numeric":
                case "real":
                    return DbType.Decimal;
                case "date":
                case "time":
                case "timestamp":
                case "datetime":
                    return DbType.DateTime;
                case "char":
                case "varchar":
                case "graphic":
                case "vargraphic":
                case "nvarchar":
                case "text":
                    return DbType.String;
                case "blob":
                    return DbType.Binary;
                default:
                    return DbType.Object;
            }            
        }
    }
}
