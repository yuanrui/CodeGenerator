using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System.Data.SQLite;
using System.Diagnostics;

namespace Banana.AutoCode.Forms
{
    public partial class DbConnBuilderPanel : Form
    {
        const string SqlServer = "Sql Server";
        const string MySql = "MySql";
        const string Oracle = "Oracle";
        const string SQLite = "SQLite";

        protected class ViewModel
        {
            public string Provider { get; set; }

            public string Server { get; set; }

            public int Port { get; set; }

            public string User { get; set; }

            public string Password { get; set; }

            public string Name { get; set; }

            public string Instance { get; set; }
        }

        public DbConnBuilderPanel()
        {
            InitializeComponent();
        }

        private DbConnectionStringBuilder GetDbConnectionStringBuilder(string provider)
        {
            switch (provider)
            {
                case "Sql Server":
                    return new System.Data.SqlClient.SqlConnectionStringBuilder();
                case "MySql":
                    return new MySql.Data.MySqlClient.MySqlConnectionStringBuilder();
                case "Oracle":
                    return new Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder();
                case "SQLite":
                    return new System.Data.SQLite.SQLiteConnectionStringBuilder();

                default:
                    break;
            }
            return null;
        }

        private string GetSqlServerConnectionString(ViewModel model)
        {
            var builder = new SqlConnectionStringBuilder();
            builder.InitialCatalog = model.Instance;
            builder.DataSource = model.Server;
            builder.UserID = model.User;
            builder.Password = model.Password;
            
            return builder.ToString();
        }

        private string GetMySqlConnectionString(ViewModel model)
        {
            var builder = new MySqlConnectionStringBuilder();
            builder.Server = model.Server;
            builder.Port = (uint)model.Port;
            builder.Database = model.Instance;
            builder.UserID = model.User;
            builder.Password = model.Password;

            return builder.ToString();
        }

        private string GetOracleConnectionString(ViewModel model)
        {
            var builder = new OracleConnectionStringBuilder();
            builder.DataSource = $"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={model.Server})(PORT={model.Port}))(CONNECT_DATA=(SERVICE_NAME={model.Instance})))";
            builder.UserID = model.User;
            builder.Password = model.Password;

            return builder.ToString();
        }

        private string GetSQLiteConnectionString(ViewModel model)
        {
            var builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = model.Instance;
            if (! string.IsNullOrWhiteSpace(model.Password))
            {
                builder.Password = model.Password;
            }

            return builder.ToString();
        }

        private ConnectionStringSettings BuildSettings()
        {
            ConnectionStringSettings settings = new ConnectionStringSettings();
            var model = GetModel();
            settings.Name = model.Name;

            switch (model.Provider)
            {
                case SqlServer:
                    settings.ConnectionString = GetSqlServerConnectionString(model);
                    settings.ProviderName = "System.Data.SqlClient";
                    break;
                case MySql:
                    settings.ConnectionString = GetMySqlConnectionString(model);
                    settings.ProviderName = "MySql.Data.MySqlClient";
                    break;
                case Oracle:
                    settings.ConnectionString = GetOracleConnectionString(model);
                    settings.ProviderName = "Oracle.ManagedDataAccess.Client";
                    break;
                case SQLite:
                    settings.ConnectionString = GetSQLiteConnectionString(model);
                    settings.ProviderName = "System.Data.SQLite";
                    break;
                default:
                    break;
            }
            return settings;
        }

        private ViewModel GetModel()
        {
            var model = new ViewModel();
            model.Provider = cboDataProvider.SelectedText;
            model.Server = txtServer.Text;
            model.User = txtUser.Text;
            model.Password = txtPassword.Text;
            var port = 0;
            int.TryParse(txtPort.Text, out port);
            model.Port = port;
            model.Name = txtName.Text;

            return model;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            var settings = BuildSettings();
            if (settings == null)
            {
                return;
            }
            var success = false;
            try
            {
                var factory = DbProviderFactories.GetFactory(settings.ProviderName);
                using (var conn = factory.CreateConnection())
                {
                    conn.ConnectionString = settings.ConnectionString;
                    conn.Open();
                    success = conn.State == ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            MessageBox.Show(success ? "Test Success" : "Test Fail");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
