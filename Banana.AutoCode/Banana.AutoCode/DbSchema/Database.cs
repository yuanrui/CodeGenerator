using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.DbSchema
{
    public class Database
    {
        public string DbName { get; set; }

        public IList<Table> Tables { get; set; } 
    }
}
