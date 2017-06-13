using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.DbSchema
{
    [Serializable]
    public class Table
    {
        private string _comment;

        public Table()
        {
            Columns = new List<Column>();
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Comment
        {
            get
            {
                return (_comment ?? String.Empty).Replace("\n", String.Empty).Trim();
            }
            set { _comment = value; }
        }

        public string DisplayName { get; set; }

        public string Owner { get; set; }

        public IList<Column> Columns { get; set; }

        public List<Column> PrimaryKeyColumns
        {
            get
            {
                if (! Columns.Any(m => m.IsPrimaryKey))
                {
                    throw new ArgumentException(Name + " no primary key");
                }

                return Columns.Where(m => m.IsPrimaryKey).ToList() ?? new List<Column>();
            }
        }

        public bool PrimaryKeyIsNumber
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

        public override string ToString()
        {
            return Owner + "." + Name;
        }
    }
}
