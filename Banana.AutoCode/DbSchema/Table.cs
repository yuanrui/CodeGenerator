using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.DbSchema
{
    [Serializable]
    public class Table
    {
        public Table()
        {
            Columns = new List<Column>();
        }

        public string Id { get; set; }

        public string Name { get; set; }
        
        public string Comment { get; set; }

        public string Owner { get; set; }

        public IList<Column> Columns { get; set; }

        public override string ToString()
        {
            return Owner + "." + Name;
        }
    }
}
