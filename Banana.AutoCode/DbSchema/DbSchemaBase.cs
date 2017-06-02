using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Banana.AutoCode.Core;
using System.Data;

namespace Banana.AutoCode.DbSchema
{
    public abstract class DbSchemaBase
    {
        protected virtual String ConnectionName { get; set; }

        public virtual DataContext Context { get; set; }

        public abstract List<Database> GetDatabases();

        public abstract List<Table> GetTables(Database db);

        public abstract List<Column> GetColumns(Table table);

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

            column.Type = GetType(column.RawType, column.Precision, column.Scale, column.IsNullable);
            column.TypeName = GetTypeName(column.RawType, column.Precision, column.Scale, column.IsNullable);
            column.DataType = GetDbType(column.RawType, column.Precision, column.Scale);

            return column;
        }

        public DbSchemaBase(String connName)
        {
            ConnectionName = connName;

            Context = DataContextScope.GetCurrent(ConnectionName).DataContext;
        }
    }
}
