using Banana.AutoCode.Resources;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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

        public DbConnBuilderPanel(ConnectionStringSettings connSetting)
        {
            InitializeComponent();

            var model = SettingToModel(connSetting);
            Init(model);
        }

        private void Init(ViewModel model)
        {
            if (model == null)
            {
                return;
            }

            cboDataProvider.Text = model.Provider;
            txtServer.Text = model.Server;
            txtName.Text = model.Name;
            txtInstance.Text = model.Instance;
            txtServer.Text = model.Server;
            txtUser.Text = model.User;
            if (model.Port > 0)
            {
                txtPort.Text = model.Port.ToString();
            }
            txtPassword.Text = model.Password;
        }

        private ViewModel SqlServerToModel(ConnectionStringSettings connSetting)
        {
            var model = new ViewModel();
            model.Name = connSetting.Name;
            model.Provider = SqlServer;

            var builder = new SqlConnectionStringBuilder(connSetting.ConnectionString);
            model.Server = builder.DataSource;
            model.User = builder.UserID;
            model.Password = builder.Password;
            model.Instance = builder.InitialCatalog;

            return model;
        }

        private ViewModel MySqlToModel(ConnectionStringSettings connSetting)
        {
            var model = new ViewModel();
            model.Name = connSetting.Name;
            model.Provider = MySql;

            var builder = new MySqlConnectionStringBuilder(connSetting.ConnectionString);
            model.Server = builder.Server;
            model.Port = (int)builder.Port;
            model.User = builder.UserID;
            model.Password = builder.Password;
            model.Instance = builder.Database;

            return model;
        }

        private ViewModel SQLiteToModel(ConnectionStringSettings connSetting)
        {
            var model = new ViewModel();
            model.Name = connSetting.Name;
            model.Provider = SQLite;

            var builder = new SQLiteConnectionStringBuilder(connSetting.ConnectionString);
            model.Password = builder.Password;
            model.Instance = builder.DataSource;

            return model;
        }

        private ViewModel OracleToModel(ConnectionStringSettings connSetting)
        {
            var model = new ViewModel();
            model.Name = connSetting.Name;
            model.Provider = Oracle;

            var builder = new OracleConnectionStringBuilder(connSetting.ConnectionString);
            var dataSource = builder.DataSource;

            model.Server = GetMatchText(dataSource, @"\(HOST=?(.+?)\)");
            var port = 1521;
            var portText = GetMatchText(dataSource, @"\(PORT=?(.+?)\)");
            if (! int.TryParse(portText, out port))
            {
                port = 1521;
            }
            model.Port = port;
            model.Instance = GetMatchText(dataSource, @"\(SERVICE_NAME=?(.+?)\)");
            model.User = builder.UserID;
            model.Password = builder.Password;
            return model;
        }

        private string GetMatchText(string input, string pattern)
        {
            var match = Regex.Match(input, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }

        private ViewModel SettingToModel(ConnectionStringSettings connSetting)
        {
            if (connSetting == null)
            {
                return null;
            }

            switch (connSetting.ProviderName)
            {
                case "System.Data.SqlClient":
                    return SqlServerToModel(connSetting);
                case "System.Data.SQLite":
                    return SQLiteToModel(connSetting);
                case "Oracle.ManagedDataAccess.Client":
                    return OracleToModel(connSetting);
                case "MySql.Data.MySqlClient":
                    return MySqlToModel(connSetting);
                default:
                    return null;
            }
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
            model.Provider = cboDataProvider.Text;
            model.Server = txtServer.Text;
            model.User = txtUser.Text;
            model.Password = txtPassword.Text;
            var port = 0;
            int.TryParse(txtPort.Text, out port);
            model.Port = port;
            model.Name = txtName.Text;
            model.Instance = txtInstance.Text;

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                model.Name = model.Provider + ":" + model.Instance;
            }

            return model;
        }

        public static void AddOrUpdateConnectionStrings(ConnectionStringSettings settings)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var collection = configFile.ConnectionStrings.ConnectionStrings;
                if (collection[settings.Name] == null)
                {
                    collection.Add(settings);
                }
                else
                {
                    collection[settings.Name].ConnectionString = settings.ConnectionString;
                    collection[settings.Name].ProviderName = settings.ProviderName;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.ConnectionStrings.SectionInformation.Name);

                Trace.WriteLine(String.Format(Localization.Save_ConnectionString_Success, settings.Name));
            }
            catch (ConfigurationErrorsException ex)
            {
                Trace.WriteLine(Localization.Save_ConnectionString_Exception + ex);
            }
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

            MessageBox.Show(success ? Localization.Test_Connection_Success : Localization.Test_Connection_Fail);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var settings = BuildSettings();
            if (settings == null)
            {
                return;
            }

            AddOrUpdateConnectionStrings(settings);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
