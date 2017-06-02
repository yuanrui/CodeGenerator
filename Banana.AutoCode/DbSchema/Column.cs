using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.DbSchema
{
    [Serializable]
    public class Column
    {        
        public string Id { get; set; }
        
        public string Name { get; set; }

        public string RawType { get; set; }

        public DbType DataType { get; set; }

        public Type Type { get; set; }

        public string TypeName { get; set; }

        public string Comment { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsForeignKey { get; set; }

        public bool IsUnique { get; set; }

        public bool IsNullable { get; set; }

        public int Length { get; set; }

        public Int16 Precision { get; set; }

        public Int16 Scale { get; set; }

        [NonSerialized]
        public Table Table;
    }
}
