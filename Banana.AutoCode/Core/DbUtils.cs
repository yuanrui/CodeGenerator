using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.Core
{
    public class DbUtils
    {
        public const string ConnectionName = "DefaultConnectionString";

        public static List<TEntity> Query<TEntity>(string sql, dynamic param = null, CommandType? cmdType = CommandType.Text)
        {
            return new DbUtils<TEntity>(ConnectionName).Query(sql, param, cmdType);
        }

        public static List<dynamic> QueryDynamic(string sql, dynamic param = null, CommandType? cmdType = CommandType.Text)
        {
            return new DbUtils<IEntityWrapper>(ConnectionName).QueryDynamic(sql, param, cmdType);
        }

        public static int Execute(string sql, dynamic param = null, CommandType? cmdType = CommandType.Text)
        {
            return new DbUtils<IEntityWrapper>(ConnectionName).Execute(sql, param, cmdType);
        }

        public static DataTable QueryDataTable(string sql)
        {
            return new DbUtils<IEntityWrapper>(ConnectionName).QueryDataTable(sql);
        }

        public static DataTable QueryDataTable(string sql, string tableName, CommandType cmdType = CommandType.Text)
        {
            return new DbUtils<IEntityWrapper>(ConnectionName).QueryDataTable(sql, tableName, cmdType);
        }

        private interface IEntityWrapper { }
    }

    public class DbUtils<TEntity>
    {
        protected readonly string ConnectionName;

        public DbUtils(string connectionName)
        {
            ConnectionName = connectionName;
        }

        public List<TEntity> Query(string sql, dynamic param = null, CommandType? cmdType = CommandType.Text)
        {
            using (DataContextScope.GetCurrent(ConnectionName).Begin())
            {
                return DataContextScope.GetCurrent(ConnectionName).DataContext.Query<TEntity>(sql, param, cmdType);
            }
        }

        public List<dynamic> QueryDynamic(string sql, dynamic param = null, CommandType? cmdType = CommandType.Text)
        {
            using (DataContextScope.GetCurrent(ConnectionName).Begin())
            {
                return DataContextScope.GetCurrent(ConnectionName).DataContext.QueryDynamic(sql, param, cmdType);
            }
        }

        public int Execute(string sql, dynamic param = null, CommandType? cmdType = CommandType.Text)
        {
            using (var ctx = DataContextScope.GetCurrent(ConnectionName).Begin(true))
            {
                var result = DataContextScope.GetCurrent(ConnectionName).DataContext.Execute(sql, param, cmdType);
                ctx.Commit();

                return result;
            }
        }

        public DataTable QueryDataTable(string sql)
        {
            using (DataContextScope.GetCurrent(ConnectionName).Begin())
            {
                return DataContextScope.GetCurrent(ConnectionName).DataContext.QueryDataTable(sql);
            }
        }

        public DataTable QueryDataTable(string sql, string tableName, CommandType cmdType = CommandType.Text)
        {
            using (DataContextScope.GetCurrent(ConnectionName).Begin())
            {
                return DataContextScope.GetCurrent(ConnectionName).DataContext.QueryDataTable(sql, tableName, cmdType);
            }
        }
    }
}
