using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.DbSchema
{
    /// <summary>
    /// 列
    /// </summary>
    public class Column
    {
        private Type type;
        private string typeName;
        /// <summary>
        /// 列ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 数据列类型 int,char,varchar,nvarchar...
        /// </summary>
        public string DBType { get; set; }
        /// <summary>
        /// .Net 支持的类型
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// .Net 支持的类型名
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 列注释
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// 表 所属表
        /// </summary>
        public Table Table { get; set; }
        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPrimaryKey { get; set; }
        /// <summary>
        /// 是否外键
        /// </summary>
        public bool IsForeignKey { get; set; }
        /// <summary>
        /// 是否允许为空
        /// </summary>
        public bool IsNullAble { get; set; }
    }
}
