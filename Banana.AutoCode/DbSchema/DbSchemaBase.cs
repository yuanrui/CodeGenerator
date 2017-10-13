using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Banana.AutoCode.Core;

namespace Banana.AutoCode.DbSchema
{
    public abstract class DbSchemaBase
    {
        protected virtual String ConnectionName { get; set; }

        public virtual DataContext Context { get; set; }
        
        public virtual String MetaDataCollectionName_Databases { get { return "Databases"; } }
        
        public virtual String MetaDataCollectionName_Tables { get { return "Tables"; } }

        public virtual String MetaDataCollectionName_Columns { get { return "Columns"; } }

        public virtual List<Database> GetDatabases()
        {
            var result = new List<Database>();

            var dt = GetSchema(MetaDataCollectionName_Databases, null);

            foreach (DataRow dr in dt.Rows)
            {
                var db = new Database();
                db.Name = dr["database_name"].ToString();

                result.Add(db);
            }

            return result;
        }

        public virtual List<Table> GetTables(Database db)
        {
            var result = new List<Table>();

            string[] restrictions = new string[4];
            restrictions[0] = db.Name;
            DataTable dt = GetSchema(MetaDataCollectionName_Tables, restrictions);

            foreach (DataRow dr in dt.Rows)
            {
                var table = new Table();
                table.Name = dr["table_name"].ToString();
                table.Owner = db.Name;

                result.Add(table);
            }

            return result;
        }

        public virtual List<Column> GetColumns(Table table)
        {
            var result = new List<Column>();

            string[] restrictions = new string[4];
            restrictions[0] = table.Owner;
            restrictions[2] = table.Name;
            DataTable dt = GetSchema(MetaDataCollectionName_Columns, restrictions);
            var index = 0;

            foreach (DataRow dr in dt.Rows)
            {
                var column = new Column();
                column.Name = dr["column_name"].ToString();
                column.Comment = column.Name;
                column.IsNullable = dr["is_nullable"].ToString() == "YES" ? true : false;
                column.RawType = dr["data_type"].ToString();
                column.Length = dr["character_maximum_length"] == DBNull.Value ? -1 : Convert.ToInt32(dr["character_maximum_length"]);
                column.Precision = dr["numeric_precision"] == DBNull.Value ? (Int16)(-1) : Convert.ToInt16(dr["numeric_precision"]);
                column.Scale = dr["numeric_scale"] == DBNull.Value ? (Int16)(-1) : Convert.ToInt16(dr["numeric_scale"]);
                
                column.Type = this.GetType(column.RawType, column.Precision, column.Scale, column.IsNullable);
                column.TypeName = this.GetTypeName(column.RawType, column.Precision, column.Scale, column.IsNullable);
                column.DataType = this.GetDbType(column.RawType, column.Precision, column.Scale);
                column.SetThriftType(GetThriftType(column.Type));
                column.Index = index++;

                result.Add(column);
            }

            return result;
        }

        public abstract Type GetType(String rawType, Int16 precision, Int16 scale, Boolean isNullable);

        public virtual String GetTypeName(String rawType, Int16 precision, Int16 scale, Boolean isNullable)
        {
            var type = GetType(rawType, precision, scale, isNullable);
            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
            {
                return nullableType.Name + "?";
            }

            return type.Name;
        }

        public abstract DbType GetDbType(String rawType, Int16 precision, Int16 scale);

        protected static Type GetTypeOf<T>(Boolean isNullable) where T : struct
        {
            if (isNullable)
            {
                return typeof(Nullable<T>);
            }

            return typeof(T);
        }

        protected virtual void FixRawType(Column column)
        { 
            
        }

        public virtual Column Fill(IDataReader reader)
        {
            var column = new Column();

            column.Id = Convert.ToString(reader.GetValue(reader.GetOrdinal("Id")));
            column.Name = reader.GetString(reader.GetOrdinal("Name"));
            column.RawType = reader.GetString(reader.GetOrdinal("RawType"));
            column.Comment = reader["Comment"] == DBNull.Value ? string.Empty : reader.GetString(reader.GetOrdinal("Comment"));

            column.IsPrimaryKey = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsPrimaryKey")));
            column.IsForeignKey = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsForeignKey")));
            column.IsUnique = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsUnique")));
            column.IsNullable = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsNullable")));

            column.Length = reader["Length"] == DBNull.Value ? 0 : Convert.ToInt32(reader.GetValue(reader.GetOrdinal("Length")));
            column.Precision = reader["Precision"] == DBNull.Value ? (Int16)0 : Convert.ToInt16(reader.GetValue(reader.GetOrdinal("Precision")));
            column.Scale = reader["Scale"] == DBNull.Value ? (Int16)0 : Convert.ToInt16(reader.GetValue(reader.GetOrdinal("Scale")));
            
            FixRawType(column);
            column.Type = GetType(column.RawType, column.Precision, column.Scale, column.IsNullable);
            column.TypeName = GetTypeName(column.RawType, column.Precision, column.Scale, column.IsNullable);
            column.DataType = GetDbType(column.RawType, column.Precision, column.Scale);
            column.SetThriftType(GetThriftType(column.Type));

            return column;
        }

        public DbSchemaBase(String connName)
        {
            ConnectionName = connName;

            Context = DataContextScope.GetCurrent(ConnectionName).DataContext;
        }

        public virtual DataTable GetSchema(String metaDataCollectionName, string[] restrictions)
        {
            DataTable resultTable;
            using (DbConnection conn = Context.DbProviderFactory.CreateConnection())
            {
                conn.ConnectionString = Context.GetConnectionString();
                conn.Open();

                if (string.IsNullOrEmpty(metaDataCollectionName))
                {
                    resultTable = conn.GetSchema();
                }
                else
                {
                    if (restrictions == null || restrictions.All(s => s == null))
                    {
                        resultTable = conn.GetSchema(metaDataCollectionName);
                    }
                    else
                    {
                        resultTable = conn.GetSchema(metaDataCollectionName, restrictions);
                    }
                }
            }

            return resultTable;
        }

        public String GetThriftType(Type type)
        {
            const String BOOL = "bool";
            const String BYTE = "byte";
            const String I16 = "i16";
            const String I32 = "i32";
            const String I64 = "i64";
            const String DOUBLE = "double";
            const String STRING = "string";
            const String BINARY = "binary";
           
            switch (type.Name)
            {
                case "String":
                    return STRING;
                case "Byte[]":
                case "SByte[]":
                    return BINARY;
                case "Boolean":
                    return BOOL;
                case "Byte":
                case "SByte":
                    return BYTE;
                case "Decimal":
                case "Double":
                case "Single":
                    return DOUBLE;
                case "Int16":
                case "UInt16":
                    return I16;
                case "Int32":
                case "UInt32":
                    return I32;
                case "Int64":
                case "UInt64":
                    return I64;
                default:
                    break;
            }

            return STRING;
        }
    }
}
