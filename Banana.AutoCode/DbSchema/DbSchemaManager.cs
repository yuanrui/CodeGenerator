using Banana.AutoCode.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace Banana.AutoCode.DbSchema
{
    public class DbSchemaManager
    {
        protected DbSchemaBase Provider;

        protected string ConnectionName;

        public DbSchemaManager(ConnectionStringSettings connSetting)
        {
            ConnectionName = connSetting.Name;
            Provider = DbSchemaFactory.Create(connSetting);
        }

        public static string GetCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            name = FixPrefixAndSuffix(name);

            if (name.IsAllUpperCase())
            {
                name = name.ToLower();
            }

            
            return name.ToCamelCase();
        }

        private static string GetPascalCase(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            name = FixPrefixAndSuffix(name);

            if (name.IsAllUpperCase())
            {
                name = name.ToLower();
            }

            return name.ToPascalCase();
        }

        private static string FixPrefixAndSuffix(string name)
        {
            foreach (var prefix in GlobalSetting.RemovePrefixList)
            {
                if (!name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                name = name.Remove(0, prefix.Count());
            }

            foreach (var suffix in GlobalSetting.RemoveSuffixList)
            {
                if (!name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                name = name.Remove(0, suffix.Count());
            }

            return name;
        }

        private void FixTables(IList<Table> tables)
        {
            if (tables == null)
            {
                return;
            }

            foreach (var table in tables)
            {
                table.DisplayName = GetPascalCase(table.Name);
            }
        }

        public virtual List<Database> GetDatabases()
        {
            try
            {
                using (var ctx = DataContextScope.GetCurrent(ConnectionName).Begin())
                {
                    var dbs = Provider.GetDatabases();

                    return dbs;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Get Databases Exception:" + ex.ToString());
            }

            return new List<Database>();
        }

        public virtual List<Database> GetComplexDatabases()
        {
            try
            {
                using (var ctx = DataContextScope.GetCurrent(ConnectionName).Begin())
                {
                    var dbs = Provider.GetDatabases();

                    if (dbs != null)
                    {
                        foreach (var db in dbs)
                        {
                            db.Tables = Provider.GetTables(db);
                            
                            FixTables(db.Tables);

                            if (db.Tables != null)
                            {
                                foreach (var table in db.Tables)
                                {
                                    table.Owner = db.Name;
                                }
                            }
                        }
                    }

                    return dbs;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Get Complex Databases Exception:" + ex.ToString());
            }

            return new List<Database>();
        }

        public virtual List<Table> GetTables(Database db)
        {
            try
            {
                using (var ctx = DataContextScope.GetCurrent(ConnectionName).Begin())
                {
                    var tables = Provider.GetTables(db);

                    if (tables != null)
                    {
                        foreach (var table in tables)
                        {
                            table.Owner = db.Name;
                        }
                    }
                    
                    FixTables(tables);

                    return tables;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Get GetTables Exception:" + ex.ToString());
            }

            return new List<Table>(); 
        }

        public virtual List<Column> GetColumns(Table table)
        {
            try
            {
                using (var ctx = DataContextScope.GetCurrent(ConnectionName).Begin())
                {
                    var columns = Provider.GetColumns(table);

                    if (columns != null)
                    {
                        table.Columns = columns;
                        var index = 0;

                        foreach (var column in columns)
                        {
                            column.Table = table;
                            column.Index = index++;
                        }
                    }

                    return columns;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Get Columns Exception:" + ex.ToString());
            }

            return new List<Column>();
        }
    }
}
