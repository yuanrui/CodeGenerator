//------------------------------------------------------------------------------
// <copyright file="PostgreSqlSchema.cs">
//    Copyright (c) 2025, https://github.com/yuanrui All rights reserved.
// </copyright>
// <author>Yuan Rui</author>
// <date>2025-05-08 18:00:00</date>
//------------------------------------------------------------------------------

using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace Banana.AutoCode.DbSchema.Provider
{
    public class PostgreSqlSchema : DbSchemaBase
    {
        public PostgreSqlSchema(string connName) : base(connName)
        {
        }
        
        public override List<Database> GetDatabases()
        {
            const String sql = @"SELECT datname AS Name 
FROM pg_database 
WHERE datname NOT LIKE 'template%' AND datname != 'postgres';";

            return Context.Query<Database>(sql);
        }

        public override List<Table> GetTables(Database db)
        {
            const string sql = @"
SELECT 
    t.table_name AS Id,
		t.table_name AS Name,
    d.description AS Comment,
		t.table_catalog AS Owner
FROM 
    information_schema.tables t
LEFT JOIN 
    pg_catalog.pg_class c ON c.relname = t.table_name
LEFT JOIN 
    pg_catalog.pg_description d ON d.objoid = c.oid AND d.objsubid = 0
WHERE 
    t.table_schema NOT IN ('pg_catalog', 'information_schema')
    AND t.table_type = 'BASE TABLE' and t.table_schema = 'public'
ORDER BY 
    t.table_name;";
            ChangeDatabase(db.Name);

            var result = Context.Query<Table>(sql, new { TableSchema = db.Name });

            return result ?? new List<Table>();
        }

        public override List<Column> GetColumns(Table table)
        {
            const string sql = @"
SELECT 
    c.ordinal_position AS Id,
    c.column_name AS Name,
    c.data_type AS RawType,
    c.character_maximum_length AS Length,
    c.numeric_precision AS Precision,
    c.numeric_scale AS Scale,
    -- c.column_default AS DefaultValue,
    c.ordinal_position AS Index,
    pg_catalog.col_description(
        (SELECT oid FROM pg_catalog.pg_class WHERE relname = @TableName), 
        c.ordinal_position
    ) AS Comment,
    CASE WHEN tc.constraint_type = 'PRIMARY KEY' THEN 1 ELSE 0 END AS IsPrimaryKey,
    CASE WHEN tc.constraint_type = 'FOREIGN KEY' THEN 1 ELSE 0 END AS IsForeignKey,
    CASE WHEN tc.constraint_type = 'UNIQUE' THEN 1 ELSE 0 END AS IsUnique,
    CASE WHEN c.is_nullable = 'YES' THEN 1 ELSE 0 END AS IsNullable
FROM 
    information_schema.columns c
LEFT JOIN 
    information_schema.key_column_usage kcu ON c.table_schema = kcu.table_schema 
    AND c.table_name = kcu.table_name 
    AND c.column_name = kcu.column_name
LEFT JOIN 
    information_schema.table_constraints tc ON kcu.constraint_name = tc.constraint_name 
    AND kcu.table_schema = tc.table_schema 
    AND kcu.table_name = tc.table_name
WHERE 
    c.table_name = @TableName
    AND c.table_schema = 'public'
ORDER BY 
    c.ordinal_position";

            ChangeDatabase(table.Owner);

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

        protected virtual void ChangeDatabase(string dbName)
        {
            var builder = new NpgsqlConnectionStringBuilder(Context.GetConnectionString());
            if (builder.Database == dbName)
            {
                return;
            }

            builder.Database = dbName;
#if NET
            builder.NoResetOnClose = true;
#endif
            var connStr = builder.ToString();
            Context.SetConnectionString(connStr);
        }

        public override DbType GetDbType(string rawType, short precision, short scale)
        {
            if (string.IsNullOrEmpty(rawType))
            {
                return DbType.String;
            }

            string dbType = rawType.ToLowerInvariant();

            switch (dbType)
            {
                case "tinyint":
                case "int1":            // openGauss Tiny integer, also called INT1
                    return DbType.Byte;
                case "smallint":
                case "smallserial":     // openGauss Two-byte auto-incrementing integer
                case "int2":
                    return DbType.Int16;
                case "integer":
                case "binary_integer":  // openGauss Alias of INTEGER.
                case "serial":          // openGauss Four-byte auto-incrementing integer
                case "int":
                case "int4":
                    return DbType.Int32;
                case "bigint":
                case "int8":
                case "bigserial":       // openGauss Eight-byte auto-incrementing integer
                    return DbType.Int64;
                case "hash16":          // openGauss Stored as an unsigned 64-bit integer
                    return DbType.UInt64;
                case "real":
                case "float4":
                    return DbType.Single;
                case "double precision":
                case "float8":
                case "binary_double":   // openGauss Alias of DOUBLE PRECISION.
                case "float":           // openGauss Floating points, inexact.
                    return DbType.Double;
                case "numeric":
                case "decimal":
                case "dec":             // openGauss
                case "int16":           // openGauss A 16-byte certificate
                case "largeserial":     // openGauss 16-byte sequence integer
                case "hash32":          // openGauss Stored as a group of 16 unsigned integer elements
                    return DbType.Decimal;
                case "money":
                    return DbType.Currency;
                case "boolean":
                case "bool":
                    return DbType.Boolean;
                case "character":
                case "char":
                case "nchar":           // openGauss Fixed-length character string
                    return DbType.StringFixedLength;
                case "character varying":
                case "varchar":
                case "varchar2":        // openGauss Variable-length string
                case "nvarchar":        // openGauss Variable-length string
                case "nvarchar2":       // openGauss Variable-length string
                case "clob":            // openGauss A big text object
                case "name":            // openGauss Internal type for object names
                    return DbType.String;
                case "text":
                    return DbType.String;
                case "bytea":
                case "raw":             // openGauss Variable-length hexadecimal string
                case "blob":            // openGauss Binary large object
                case "byteawithoutorderwithequalcol":   // openGauss Variable-length binary character string
                case "byteawithoutordercol":            // openGauss Variable-length binary character string
                case "_byteawithoutorderwithequalcol":  // openGauss Variable-length binary character string
                case "_byteawithoutordercol":           // openGauss Variable-length binary character string
                    return DbType.Binary;
                case "timestamp":
                case "timestamp without time zone":
                case "smalldatetime":                   // openGauss Date and time (without time zone)
                case "abstime":                         // openGauss Date and time
                    return DbType.DateTime;
                case "timestamp with time zone":
                case "timestamptz":
                    return DbType.DateTimeOffset;
                case "date":
                    return DbType.Date;
                case "time":
                case "time without time zone":
                case "time with time zone":
                case "timetz":
                case "reltime":                         // openGauss Relative time interval
                    return DbType.Time;
                case "uuid":
                    return DbType.Guid;
                case "json":
                case "jsonb":
                    return DbType.String;
                case "xml":
                    return DbType.Xml;
                case "interval":
                    return DbType.Time;
                default:
                    return DbType.String;
            }
        }

        public override Type GetType(string rawType, short precision, short scale, bool isNullable)
        {
            if (string.IsNullOrEmpty(rawType))
            {
                return typeof(string);
            }

            string dbType = rawType.ToLowerInvariant();
            Type type;

			// https://www.postgresql.org/docs/current/datatype.html
			// https://docs.opengauss.org/en/docs/6.0.0/docs/SQLReference/data-types.html
            switch (dbType)
            {
                case "tinyint":         // openGauss Tiny integer, also called INT1
                case "int1":            // openGauss Tiny integer, also called INT1
                    type = typeof(byte);
                    break;
                case "smallint":
                case "smallserial":     // openGauss Two-byte auto-incrementing integer
                case "int2":
                    type = typeof(short);
                    break;
                case "integer":
                case "binary_integer":  // openGauss Alias of INTEGER.
                case "serial":          // openGauss Four-byte auto-incrementing integer
                case "int":
                case "int4":
                    type = typeof(int);
                    break;
                case "bigint":
                case "int8":
                case "bigserial":       // openGauss Eight-byte auto-incrementing integer
                    type = typeof(long);
                    break;
                case "hash16":
                    type = typeof(ulong);   // openGauss Stored as an unsigned 64-bit integer
                    break;
                case "real":
                case "float4":
                    type = typeof(float);
                    break;
                case "double precision":
                case "float8":
                case "binary_double":   // openGauss Alias of DOUBLE PRECISION.
                case "float":           // openGauss Floating points, inexact. The value range of p (precision) is [1,53]. NOTE: p is the precision, indicating the total decimal digits.
                    type = typeof(double);
                    break;
                case "numeric":
                case "decimal":
                case "dec":             // openGauss The value range of p (precision) is [1,1000], and the value range of s (scale) is [0,p]. NOTE:p indicates the total digits, and s indicates the decimal digit.
                case "int16":           // openGauss A 16-byte certificate cannot be used to create tables.
                case "largeserial":     // openGauss By default, a 16-byte sequence integer is inserted. The actual data type is the same as that of numeric.
                case "hash32":          // openGauss Stored as a group of 16 unsigned integer elements
                    type = typeof(decimal);
                    break;
                case "money":
                    type = typeof(decimal);
                    break;
                case "boolean":
                case "bool":
                    type = typeof(bool);
                    break;
                case "character":
                case "char":
                case "character varying":
                case "varchar":
                case "name":            // openGauss Internal type for object names
                case "nchar":           // openGauss Fixed-length character string, blank padded. n indicates the string length. If it is not specified, the default precision 1 is used.
                case "varchar2":        // openGauss Variable-length string. It is the alias of the VARCHAR(n) type. n indicates the string length.
                case "nvarchar":        // openGauss Variable-length string. It is the alias of the NVARCHAR2(n) type. n indicates the string length.
                case "nvarchar2":       // openGauss Variable-length string. n indicates the string length.
                case "clob":            // openGauss A big text object. It is the alias of the TEXT type.
                case "text":
                    type = typeof(string);
                    break;
                case "bytea":
                case "raw":             // openGauss Variable-length hexadecimal string. NOTE: Column storage cannot be used for the raw type.
                case "blob":            // openGauss Binary large object (BLOB). NOTE: Column storage cannot be used for the BLOB type.
                case "byteawithoutorderwithequalcol":   // openGauss Variable-length binary character string (new type for the encryption feature. If the encryption type of the encrypted column is specified as deterministic encryption, the column type is BYTEAWITHOUTORDERWITHEQUALCOL). The original data type is displayed when the encrypted table is printed by running the meta command.
                case "byteawithoutordercol":            // openGauss Variable-length binary character string (new type for the encryption feature. If the encryption type of the encrypted column is specified as random encryption, the column type is BYTEAWITHOUTORDERCOL). The original data type is displayed when the encrypted table is printed by running the meta command.
                case "_byteawithoutorderwithequalcol":  // openGauss Variable-length binary character string, which is a new type for the encryption feature.
                case "_byteawithoutordercol":           // openGauss Variable-length binary character string, which is a new type for the encryption feature.
                    type = typeof(byte[]);
                    break;
                case "timestamp":
                case "timestamp without time zone":
                case "timestamp with time zone":
                case "timestamptz":
                case "date":
                case "smalldatetime":                   // openGauss Date and time (without time zone). The precision is minute.A duration between 30s and 60s is rounded into 1 minute.
                case "abstime":                         // openGauss Date and time. The format is as follows:YYYY-MM-DD hh:mm:ss+timezone The value range is from 1901-12-13 20:45:53 GMT to 2038-01-18 23:59:59 GMT. The precision is second.
                    type = typeof(DateTime);
                    break;
                case "time":
                case "time without time zone":
                case "time with time zone":
                case "timetz":
                case "interval":
                case "reltime":                         // openGauss Relative time interval. The format is as follows:X years X months X days XX:XX:XX The Julian calendar is used. It specifies that a year has 365.25 days and a month has 30 days. The relative time interval needs to be calculated based on the input value. The output format is POSTGRES.
                    type = typeof(TimeSpan);
                    break;
                case "uuid":
                    type = typeof(Guid);
                    break;
                case "json":
                case "jsonb":
                case "xml":
                default:
                    type = typeof(string);
                    break;
            }

            // process nullable type
            if (isNullable && type.IsValueType)
            {
                type = typeof(Nullable<>).MakeGenericType(type);
            }

            return type;
        }
    }
}
