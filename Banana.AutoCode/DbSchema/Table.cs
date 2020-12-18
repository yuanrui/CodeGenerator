using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.DbSchema
{
    [Serializable]
    public class Table
    {
        private String _comment;

        public Table()
        {
            Columns = new List<Column>();
        }

        public String Id { get; set; }

        public String Name { get; set; }

        public String CamelCase
        {
            get
            {
                var name = this.DisplayName;
                if (string.IsNullOrEmpty(name))
                {
                    return string.Empty;
                }

                return name.Substring(0, 1).ToLower() + name.Substring(1);
            }
        }

        public String Comment
        {
            get
            {
                return (_comment ?? String.Empty).Replace("\n", String.Empty).Trim();
            }
            set { _comment = value; }
        }

        public String DisplayName { get; set; }

        public String Owner { get; set; }

        public IList<Column> Columns { get; set; }

        public List<Column> PrimaryKeyColumns
        {
            get
            {
                return Columns.Where(m => m.IsPrimaryKey).ToList() ?? new List<Column>();
            }
        }

        public Boolean PrimaryKeyIsNumber
        {
            get
            {
                if (PrimaryKeyColumns.Count != 1)
                {
                    return false;
                }

                var typeName = PrimaryKeyColumns.First().TypeName;
                switch (typeName)
                {
                    case "Byte":
                    case "Int16":
                    case "Int32":
                    case "Int64":
                    case "Single":
                    case "Double":
                    case "Decimal":
                        return true;
                    default:
                        return false;
                }
            }
        }

        public List<Column> NonPrimaryKeyColumns
        {
            get
            {
                return Columns.Where(m => !m.IsPrimaryKey).ToList() ?? new List<Column>();
            }
        }

        public override String ToString()
        {
            return Owner + "." + Name;
        }
    }
}
