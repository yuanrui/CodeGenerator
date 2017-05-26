using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Dapper;

namespace Banana.AutoCode.Core
{
    public class DataContext : IDisposable
    {
        protected const string ConnectionName = "DefaultConnectionString";
        private DbConnection _dbConnection;

        private DbTransaction _dbTransaction;

        private readonly DbProviderFactory _dbProviderFactory;

        private DatabaseWrapper _database;

        public DatabaseWrapper DatabaseObject
        {
            get { return _database; }
        }

        //public DbConnection ConnectionObject
        //{
        //    get
        //    {
        //        return _dbConnection;
        //    }
        //}

        //public DbTransaction TransactionObject
        //{
        //    get { return _dbTransaction; }
        //}
        
        internal DbProviderFactory DbProviderFactory
        {
            get { return _dbProviderFactory; }
        }

        public Boolean IsDisposed { get; private set; }
        public Boolean IsTransactionOpened { get; private set; }

        public DataContext() : this(ConnectionName)
        {
            //_database = DatabaseFactory.CreateDatabase();
            //_dbConnection = _database.CreateConnection();
        }

        public DataContext(string connectionName)
        {
            var connSetting = ConfigurationManager.ConnectionStrings[connectionName];
            _dbProviderFactory = DbProviderFactories.GetFactory(connSetting.ProviderName);
            _database = new DatabaseWrapper(_dbProviderFactory);
            _dbConnection = _dbProviderFactory.CreateConnection();
            _dbConnection.ConnectionString = connSetting.ConnectionString;
        }

        public void OpenConnection()
        {
            if (_dbConnection.State == ConnectionState.Open)
            {
                return;
            }

            //if (IsDisposed)
            //{
            //    _dbConnection = _database.CreateConnection();
            //}

            _dbConnection.Open();

            IsDisposed = false;
        }

        public void BeginTransaction()
        {
            _dbTransaction = _dbConnection.BeginTransaction();
            IsTransactionOpened = true;
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            _dbTransaction = _dbConnection.BeginTransaction(isolationLevel);
            IsTransactionOpened = true;
        }

        public void CommitTransaction()
        {
            if (!IsTransactionOpened)
            {
                throw new InvalidOperationException("事务未开启, 无法提交");
            }

            try
            {
                _dbTransaction.Commit();
            }
            finally
            {
                _dbTransaction.Dispose();
                _dbConnection.Close();
                IsTransactionOpened = false;
            }
        }

        public void RollbackTransaction()
        {
            if (!IsTransactionOpened)
            {
                throw new InvalidOperationException("事务未开启, 无法回滚");
            }

            try
            {
                _dbTransaction.Rollback();
            }
            finally
            {
                _dbTransaction.Dispose();
                _dbConnection.Close();
                IsTransactionOpened = false;
            }
        }

        public IDataReader ExecuteReader(DbCommand dbCommand)
        {
            dbCommand.Connection = _dbConnection;
            dbCommand.Transaction = _dbTransaction;

            return dbCommand.ExecuteReader();
        }

        public Object ExecuteScalar(DbCommand dbCommand)
        {
            dbCommand.Connection = _dbConnection;
            dbCommand.Transaction = _dbTransaction;

            return dbCommand.ExecuteScalar();
        }

        public Int32 ExecuteNonQuery(DbCommand dbCommand)
        {
            dbCommand.Connection = _dbConnection;
            dbCommand.Transaction = _dbTransaction;

            return dbCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Sample query
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        public List<TEntity> Query<TEntity>(string sql, dynamic param = null, CommandType? cmdType = CommandType.Text)
        {
            return _dbConnection.Query<TEntity>(sql, (object)param, transaction: _dbTransaction, commandType: cmdType).ToList();
        }

        /// <summary>
        /// Query one to one 
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="splitOn"></param>
        /// <param name="param"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        public List<TFirst> Query<TFirst, TSecond>(string sql, Func<TFirst, TSecond, TFirst> map, string splitOn, dynamic param = null, CommandType? cmdType = CommandType.Text)
        {
            return _dbConnection.Query<TFirst, TSecond, TFirst>(sql, map, (object)param, transaction: _dbTransaction, splitOn: splitOn, commandType: cmdType).ToList();
        }

        /// <summary>
        /// Query one to many
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TChild"></typeparam>
        /// <typeparam name="TParentKey"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parentKeySelector"></param>
        /// <param name="childSelector"></param>
        /// <param name="splitOn"></param>
        /// <param name="param"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        public List<TParent> Query<TParent, TChild, TParentKey>(string sql, Func<TParent, TParentKey> parentKeySelector, Func<TParent, ICollection<TChild>> childSelector, string splitOn = "ID", dynamic param = null, CommandType? cmdType = CommandType.Text)
        {
            var cache = new Dictionary<TParentKey, TParent>();

            _dbConnection.Query<TParent, TChild, TParent>(sql, (parent, child) =>
            {
                if (!cache.ContainsKey(parentKeySelector(parent)))
                {
                    cache.Add(parentKeySelector(parent), parent);
                }

                TParent cachedParent = cache[parentKeySelector(parent)];
                var children = childSelector(cachedParent);
                children.Add(child);
                return cachedParent;
            }, param as object, _dbTransaction, splitOn: splitOn, commandType: cmdType);

            return cache.Values.ToList();
        }

        public Tuple<IEnumerable<TFirst>, IEnumerable<TSecond>> QueryMultiple<TFirst, TSecond>(string sql, dynamic param = null, CommandType? cmdType = CommandType.Text)
        {
            SqlMapper.GridReader gridReader = _dbConnection.QueryMultiple(sql, (object)param, transaction: _dbTransaction, commandType: cmdType);

            using (gridReader)
            {
                return Tuple.Create(gridReader.Read<TFirst>(), gridReader.Read<TSecond>());
            }
        }

        public List<dynamic> QueryDynamic(string sql, dynamic param = null, CommandType? cmdType = CommandType.Text)
        {
            return _dbConnection.Query(sql, (object)param, transaction: _dbTransaction, commandType: cmdType).ToList();
        }

        public DataTable QueryDataTable(string sql)
        {
            return QueryDataTable(sql, string.Empty);
        }

        public DataTable QueryDataTable(string sql, string tableName, CommandType cmdType = CommandType.Text)
        {
            using (var cmd = _dbConnection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = cmdType;
                
                using (var adapter = _dbProviderFactory.CreateDataAdapter())
                {
                    var table = string.IsNullOrEmpty(tableName) ? new DataTable() : new DataTable(tableName);

                    adapter.SelectCommand = cmd;
                    adapter.Fill(table);

                    return table;
                }
            }
        }

        public int Execute(string sql, dynamic param = null, CommandType? cmdType = CommandType.Text)
        {
            return _dbConnection.Execute(sql, (object)param, transaction: _dbTransaction, commandType: cmdType);
        }

        public void Dispose()
        {
            DoDispose(false);
        }

        internal void DoDispose(bool disposeConnection)
        {
            if (!IsDisposed)
            {
                if (_dbTransaction != null)
                {
                    _dbTransaction.Dispose();
                }

                _dbConnection.Close();

                IsDisposed = true;
            }

            if (disposeConnection)
            {
                _dbConnection.Dispose();
                IsDisposed = true;
            }
        }
    }
}
