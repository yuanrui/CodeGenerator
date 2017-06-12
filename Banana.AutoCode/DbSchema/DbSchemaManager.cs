using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Banana.AutoCode.Core;
using System.Diagnostics;

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

        private void FixTables(List<Table> tables)
        {
            if (tables == null)
            {
                return;
            }

            foreach (var table in tables)
            {
                table.DisplayName = table.Name;
                foreach (var prefix in GlobalSetting.RemovePrefixList)
                {
                    if (table.Name.StartsWith(prefix))
                    {
                        table.DisplayName = table.Name.Remove(0, prefix.Count());
                    }

                }

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

                        foreach (var column in columns)
                        {
                            column.Table = table;
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
