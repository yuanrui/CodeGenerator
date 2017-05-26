using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.Core
{
    /// <summary>
    /// Copy From EnterpriseLibrary.Data.Database
    /// The Database class can replace DatabaseWrapper
    /// </summary>
    public class DatabaseWrapper
    {
        readonly DbProviderFactory dbProviderFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class with a connection string,
        /// a <see cref="DbProviderFactory"/> and an <see cref="IDataInstrumentationProvider"/>.
        /// </summary>
        /// <param name="connectionString">The connection string for the database.</param>
        /// <param name="dbProviderFactory">A <see cref="DbProviderFactory"/> object.</param>
        /// <param name="instrumentationProvider">Instrumentation provider to use.</param>
        public DatabaseWrapper(DbProviderFactory dbProviderFactory)
        {
            this.dbProviderFactory = dbProviderFactory;
        }

        /// <summary>
        /// <para>Gets the DbProviderFactory used by the database instance.</para>
        /// </summary>
        /// <seealso cref="DbProviderFactory"/>
        public DbProviderFactory DbProviderFactory
        {
            get { return dbProviderFactory; }
        }

        /// <summary>
        /// Adds a new In <see cref="DbParameter"/> object to the given <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The command to add the in parameter.</param>
        /// <param name="name"><para>The name of the parameter.</para></param>
        /// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>                
        /// <remarks>
        /// <para>This version of the method is used when you can have the same parameter object multiple times with different values.</para>
        /// </remarks>        
        public void AddInParameter(DbCommand command,
                                   string name,
                                   DbType dbType)
        {
            AddParameter(command, name, dbType, ParameterDirection.Input, String.Empty, DataRowVersion.Default, null);
        }

        /// <summary>
        /// Adds a new In <see cref="DbParameter"/> object to the given <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The commmand to add the parameter.</param>
        /// <param name="name"><para>The name of the parameter.</para></param>
        /// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>                
        /// <param name="value"><para>The value of the parameter.</para></param>      
        public void AddInParameter(DbCommand command,
                                   string name,
                                   DbType dbType,
                                   object value)
        {
            AddParameter(command, name, dbType, ParameterDirection.Input, String.Empty, DataRowVersion.Default, value);
        }

        /// <summary>
        /// Adds a new In <see cref="DbParameter"/> object to the given <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The command to add the parameter.</param>
        /// <param name="name"><para>The name of the parameter.</para></param>
        /// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>                
        /// <param name="sourceColumn"><para>The name of the source column mapped to the DataSet and used for loading or returning the value.</para></param>
        /// <param name="sourceVersion"><para>One of the <see cref="DataRowVersion"/> values.</para></param>
        public void AddInParameter(DbCommand command,
                                   string name,
                                   DbType dbType,
                                   string sourceColumn,
                                   DataRowVersion sourceVersion)
        {
            AddParameter(command, name, dbType, 0, ParameterDirection.Input, true, 0, 0, sourceColumn, sourceVersion, null);
        }

        /// <summary>
        /// Adds a new Out <see cref="DbParameter"/> object to the given <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The command to add the out parameter.</param>
        /// <param name="name"><para>The name of the parameter.</para></param>
        /// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>        
        /// <param name="size"><para>The maximum size of the data within the column.</para></param>        
        public void AddOutParameter(DbCommand command,
                                    string name,
                                    DbType dbType,
                                    int size)
        {
            AddParameter(command, name, dbType, size, ParameterDirection.Output, true, 0, 0, String.Empty, DataRowVersion.Default, DBNull.Value);
        }

        /// <summary>
        /// Adds a new In <see cref="DbParameter"/> object to the given <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The command to add the parameter.</param>
        /// <param name="name"><para>The name of the parameter.</para></param>
        /// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>
        /// <param name="size"><para>The maximum size of the data within the column.</para></param>
        /// <param name="direction"><para>One of the <see cref="ParameterDirection"/> values.</para></param>
        /// <param name="nullable"><para>Avalue indicating whether the parameter accepts <see langword="null"/> (<b>Nothing</b> in Visual Basic) values.</para></param>
        /// <param name="precision"><para>The maximum number of digits used to represent the <paramref name="value"/>.</para></param>
        /// <param name="scale"><para>The number of decimal places to which <paramref name="value"/> is resolved.</para></param>
        /// <param name="sourceColumn"><para>The name of the source column mapped to the DataSet and used for loading or returning the <paramref name="value"/>.</para></param>
        /// <param name="sourceVersion"><para>One of the <see cref="DataRowVersion"/> values.</para></param>
        /// <param name="value"><para>The value of the parameter.</para></param>       
        public virtual void AddParameter(DbCommand command,
                                         string name,
                                         DbType dbType,
                                         int size,
                                         ParameterDirection direction,
                                         bool nullable,
                                         byte precision,
                                         byte scale,
                                         string sourceColumn,
                                         DataRowVersion sourceVersion,
                                         object value)
        {
            if (command == null) throw new ArgumentNullException("command");

            DbParameter parameter = CreateParameter(name, dbType, size, direction, nullable, precision, scale, sourceColumn, sourceVersion, value);
            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// <para>Adds a new instance of a <see cref="DbParameter"/> object to the command.</para>
        /// </summary>
        /// <param name="command">The command to add the parameter.</param>
        /// <param name="name"><para>The name of the parameter.</para></param>
        /// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>        
        /// <param name="direction"><para>One of the <see cref="ParameterDirection"/> values.</para></param>                
        /// <param name="sourceColumn"><para>The name of the source column mapped to the DataSet and used for loading or returning the <paramref name="value"/>.</para></param>
        /// <param name="sourceVersion"><para>One of the <see cref="DataRowVersion"/> values.</para></param>
        /// <param name="value"><para>The value of the parameter.</para></param>    
        public void AddParameter(DbCommand command,
                                 string name,
                                 DbType dbType,
                                 ParameterDirection direction,
                                 string sourceColumn,
                                 DataRowVersion sourceVersion,
                                 object value)
        {
            AddParameter(command, name, dbType, 0, direction, false, 0, 0, sourceColumn, sourceVersion, value);
        }

        void AssignParameterValues(DbCommand command,
                                   object[] values)
        {
            int parameterIndexShift = UserParametersStartIndex(); // DONE magic number, depends on the database
            for (int i = 0; i < values.Length; i++)
            {
                IDataParameter parameter = command.Parameters[i + parameterIndexShift];

                // There used to be code here that checked to see if the parameter was input or input/output
                // before assigning the value to it. We took it out because of an operational bug with
                // deriving parameters for a stored procedure. It turns out that output parameters are set
                // to input/output after discovery, so any direction checking was unneeded. Should it ever
                // be needed, it should go here, and check that a parameter is input or input/output before
                // assigning a value to it.
                SetParameterValue(command, parameter.ParameterName, values[i]);
            }
        }

        static DbTransaction BeginTransaction(DbConnection connection)
        {
            DbTransaction tran = connection.BeginTransaction();
            return tran;
        }

        /// <summary>
        /// Builds a value parameter name for the current database.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>A correctly formated parameter name.</returns>
        public virtual string BuildParameterName(string name)
        {
            return name;
        }

        static void CommitTransaction(IDbTransaction tran)
        {
            tran.Commit();
        }

        /// <summary>
        /// Configures a given <see cref="DbParameter"/>.
        /// </summary>
        /// <param name="param">The <see cref="DbParameter"/> to configure.</param>
        /// <param name="name"><para>The name of the parameter.</para></param>
        /// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>
        /// <param name="size"><para>The maximum size of the data within the column.</para></param>
        /// <param name="direction"><para>One of the <see cref="ParameterDirection"/> values.</para></param>
        /// <param name="nullable"><para>Avalue indicating whether the parameter accepts <see langword="null"/> (<b>Nothing</b> in Visual Basic) values.</para></param>
        /// <param name="precision"><para>The maximum number of digits used to represent the <paramref name="value"/>.</para></param>
        /// <param name="scale"><para>The number of decimal places to which <paramref name="value"/> is resolved.</para></param>
        /// <param name="sourceColumn"><para>The name of the source column mapped to the DataSet and used for loading or returning the <paramref name="value"/>.</para></param>
        /// <param name="sourceVersion"><para>One of the <see cref="DataRowVersion"/> values.</para></param>
        /// <param name="value"><para>The value of the parameter.</para></param>  
        protected virtual void ConfigureParameter(DbParameter param,
                                                  string name,
                                                  DbType dbType,
                                                  int size,
                                                  ParameterDirection direction,
                                                  bool nullable,
                                                  byte precision,
                                                  byte scale,
                                                  string sourceColumn,
                                                  DataRowVersion sourceVersion,
                                                  object value)
        {
            param.DbType = dbType;
            param.Size = size;
            param.Value = value ?? DBNull.Value;
            param.Direction = direction;
            param.IsNullable = nullable;
            param.SourceColumn = sourceColumn;
            param.SourceVersion = sourceVersion;
        }

        DbCommand CreateCommandByCommandType(CommandType commandType,
                                             string commandText)
        {
            DbCommand command = dbProviderFactory.CreateCommand();
            command.CommandType = commandType;
            command.CommandText = commandText;

            return command;
        }

        /// <summary>
        /// <para>Adds a new instance of a <see cref="DbParameter"/> object.</para>
        /// </summary>
        /// <param name="name"><para>The name of the parameter.</para></param>
        /// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>
        /// <param name="size"><para>The maximum size of the data within the column.</para></param>
        /// <param name="direction"><para>One of the <see cref="ParameterDirection"/> values.</para></param>
        /// <param name="nullable"><para>Avalue indicating whether the parameter accepts <see langword="null"/> (<b>Nothing</b> in Visual Basic) values.</para></param>
        /// <param name="precision"><para>The maximum number of digits used to represent the <paramref name="value"/>.</para></param>
        /// <param name="scale"><para>The number of decimal places to which <paramref name="value"/> is resolved.</para></param>
        /// <param name="sourceColumn"><para>The name of the source column mapped to the DataSet and used for loading or returning the <paramref name="value"/>.</para></param>
        /// <param name="sourceVersion"><para>One of the <see cref="DataRowVersion"/> values.</para></param>
        /// <param name="value"><para>The value of the parameter.</para></param>  
        /// <returns>A newly created <see cref="DbParameter"/> fully initialized with given parameters.</returns>
        protected DbParameter CreateParameter(string name,
                                              DbType dbType,
                                              int size,
                                              ParameterDirection direction,
                                              bool nullable,
                                              byte precision,
                                              byte scale,
                                              string sourceColumn,
                                              DataRowVersion sourceVersion,
                                              object value)
        {
            DbParameter param = CreateParameter(name);
            ConfigureParameter(param, name, dbType, size, direction, nullable, precision, scale, sourceColumn, sourceVersion, value);
            return param;
        }

        /// <summary>
        /// <para>Adds a new instance of a <see cref="DbParameter"/> object.</para>
        /// </summary>
        /// <param name="name"><para>The name of the parameter.</para></param>
        /// <returns><para>An unconfigured parameter.</para></returns>
        protected DbParameter CreateParameter(string name)
        {
            DbParameter param = dbProviderFactory.CreateParameter();
            param.ParameterName = BuildParameterName(name);

            return param;
        }

        /// <summary>
        /// Does this <see cref='Database'/> object support parameter discovery?
        /// </summary>
        /// <value>Base class always returns false.</value>
        public virtual bool SupportsParemeterDiscovery
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a DbDataAdapter with Standard update behavior.
        /// </summary>
        /// <returns>A <see cref="DbDataAdapter"/>.</returns>
        /// <seealso cref="DbDataAdapter"/>
        /// <devdoc>
        /// Created this new, public method instead of modifying the protected, abstract one so that there will be no
        /// breaking changes for any currently derived Database class.
        /// </devdoc>
        public DbDataAdapter GetDataAdapter()
        {
            DbDataAdapter adapter = dbProviderFactory.CreateDataAdapter();

            return adapter;
        }

        /// <summary>
        /// Gets a parameter value.
        /// </summary>
        /// <param name="command">The command that contains the parameter.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public virtual object GetParameterValue(DbCommand command,
                                                string name)
        {
            if (command == null) throw new ArgumentNullException("command");

            return command.Parameters[BuildParameterName(name)].Value;
        }

        /// <summary>
        /// <para>Creates a <see cref="DbCommand"/> for a SQL query.</para>
        /// </summary>
        /// <param name="query"><para>The text of the query.</para></param>        
        /// <returns><para>The <see cref="DbCommand"/> for the SQL query.</para></returns>        
        public DbCommand GetSqlStringCommand(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentException("The value can not be null or an empty string.", "query");

            return CreateCommandByCommandType(CommandType.Text, query);
        }

        /// <summary>
        /// <para>Creates a <see cref="DbCommand"/> for a stored procedure.</para>
        /// </summary>
        /// <param name="storedProcedureName"><para>The name of the stored procedure.</para></param>
        /// <returns><para>The <see cref="DbCommand"/> for the stored procedure.</para></returns>       
        public virtual DbCommand GetStoredProcCommand(string storedProcedureName)
        {
            if (string.IsNullOrEmpty(storedProcedureName)) throw new ArgumentException("The value can not be null or an empty string.", "storedProcedureName");

            return CreateCommandByCommandType(CommandType.StoredProcedure, storedProcedureName);
        }

        /// <summary>
        /// <para>Assigns a <paramref name="connection"/> to the <paramref name="command"/> and discovers parameters if needed.</para>
        /// </summary>
        /// <param name="command"><para>The command that contains the query to prepare.</para></param>
        /// <param name="connection">The connection to assign to the command.</param>
        protected static void PrepareCommand(DbCommand command,
                                             DbConnection connection)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (connection == null) throw new ArgumentNullException("connection");

            command.Connection = connection;
        }

        /// <summary>
        /// <para>Assigns a <paramref name="transaction"/> to the <paramref name="command"/> and discovers parameters if needed.</para>
        /// </summary>
        /// <param name="command"><para>The command that contains the query to prepare.</para></param>
        /// <param name="transaction">The transaction to assign to the command.</param>
        protected static void PrepareCommand(DbCommand command,
                                             DbTransaction transaction)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (transaction == null) throw new ArgumentNullException("transaction");

            PrepareCommand(command, transaction.Connection);
            command.Transaction = transaction;
        }

        static void RollbackTransaction(IDbTransaction tran)
        {
            tran.Rollback();
        }

        /// <summary>
        /// Determines if the number of parameters in the command matches the array of parameter values.
        /// </summary>
        /// <param name="command">The <see cref="DbCommand"/> containing the parameters.</param>
        /// <param name="values">The array of parameter values.</param>
        /// <returns><see langword="true"/> if the number of parameters and values match; otherwise, <see langword="false"/>.</returns>
        protected virtual bool SameNumberOfParametersAndValues(DbCommand command,
                                                               object[] values)
        {
            int numberOfParametersToStoredProcedure = command.Parameters.Count;
            int numberOfValuesProvidedForStoredProcedure = values.Length;
            return numberOfParametersToStoredProcedure == numberOfValuesProvidedForStoredProcedure;
        }

        /// <summary>
        /// Sets a parameter value.
        /// </summary>
        /// <param name="command">The command with the parameter.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        public virtual void SetParameterValue(DbCommand command,
                                              string parameterName,
                                              object value)
        {
            if (command == null) throw new ArgumentNullException("command");

            command.Parameters[BuildParameterName(parameterName)].Value = value ?? DBNull.Value;
        }

        /// <summary>
        /// Sets the RowUpdated event for the data adapter.
        /// </summary>
        /// <param name="adapter">The <see cref="DbDataAdapter"/> to set the event.</param>
        protected virtual void SetUpRowUpdatedEvent(DbDataAdapter adapter) { }

        /// <summary>
        /// Returns the starting index for parameters in a command.
        /// </summary>
        /// <returns>The starting index for parameters in a command.</returns>
        protected virtual int UserParametersStartIndex()
        {
            return 0;
        }
    }
}
