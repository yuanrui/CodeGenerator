using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.DbSchema.Provider
{
    public class SQLiteSchema : DbSchemaBase
    {
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
                    Name = "Main" 
                }
            };
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
                case "numeric":
                case "real":
                    return GetTypeOf<Decimal>(isNullable);
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
