using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.DbSchema
{
    /// <summary>
    /// 表
    /// </summary>
    public class Table
    {
        public Table()
        {
            Columns = new List<Column>();
        }

        /// <summary>
        /// 存放表的唯一ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 表名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 表名注释
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Table下面的列集合
        /// </summary>
        public IList<Column> Columns { get; set; }
    }
}
