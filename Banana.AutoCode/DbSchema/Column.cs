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
        private String _comment;

        public String Id { get; set; }
        
        public String Name { get; set; }

        public String RawType { get; set; }

        public DbType DataType { get; set; }

        public Type Type { get; set; }

        public String TypeName { get; set; }

        public String Comment
        {
            get
            {
                return (String.IsNullOrEmpty(_comment) ? Name : _comment).Replace("\n", String.Empty).Trim();
            }
            set { _comment = value; }
        }

        public Boolean IsPrimaryKey { get; set; }

        public Boolean IsForeignKey { get; set; }

        public Boolean IsUnique { get; set; }

        public Boolean IsNullable { get; set; }

        public Int32 Length { get; set; }

        public Int16 Precision { get; set; }

        public Int16 Scale { get; set; }

        public Int32 Index { get; set; }

        [NonSerialized]
        public Table Table;
    }
}
