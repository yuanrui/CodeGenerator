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
                case "int":
                case "integer":
                    return GetTypeOf<Int64>(isNullable);
                case "numeric":
                case "real":
                    return GetTypeOf<Decimal>(isNullable);
                case "datetime":
                    return GetTypeOf<DateTime>(isNullable);
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
                case "int":
                case "integer":
                    return DbType.Int64;
                case "numeric":
                case "real":
                    return DbType.Decimal;
                case "datetime":
                    return DbType.DateTime;
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
