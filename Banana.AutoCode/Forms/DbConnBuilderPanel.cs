using Banana.AutoCode.DbSchema;
using Banana.AutoCode.Resources;
#if NET
using MySqlConnector;
#else
using MySql.Data.MySqlClient;
#endif
using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Resources;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Banana.AutoCode.Forms
{
    public partial class DbConnBuilderPanel : Form
    {
        protected ResourceManager ResourceMgr = new ResourceManager(typeof(DbConnBuilderPanel));
        private DbPanel _dbPanel;

        public DbConnBuilderPanel()
        {
            InitializeComponent();
        }

        public DbConnBuilderPanel(DbPanel dbPanel) : this()
        {
            _dbPanel = dbPanel;
            InitUIValues();
        }

        public DbConnBuilderPanel(DbPanel dbPanel, ConnectionStringSettings connSetting) : this()
        {
            _dbPanel = dbPanel;

            InitUIValues();
            var model = DbProviderConfig.CreateDbViewModel(connSetting);
            InitModel(model);
        }

        private void InitUIValues()
        {
            cboDataProvider.Items.Clear();
            var dataProviders = DbProviderConfig.GetDataProviders();
            foreach (var item in dataProviders)
            {
                cboDataProvider.Items.Add(item);
            }
        }

        private void InitModel(DbViewModel model)
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

        private ConnectionStringSettings BuildSettings()
        {
            var model = GetModel();

            return DbProviderConfig.ToSettings(model) ?? new ConnectionStringSettings() { Name = model?.Name }; 
        }

        private DbViewModel GetModel()
        {
            var model = new DbViewModel();
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

        private void RefreshConnStrings()
        {
            if (_dbPanel == null)
            {
                return;
            }

            _dbPanel.InitConnStrings();
        }

        protected void AddOrUpdateConnectionStrings(ConnectionStringSettings settings)
        {
            var title = ResourceMgr.GetString("TestConnTitle") ?? "Test Result";
            var success = false;
            var msg = string.Empty;
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
                success = true;
                RefreshConnStrings();
                msg = String.Format(Localization.Save_ConnectionString_Success, settings.Name);
                Trace.WriteLine(msg);
            }
            catch (ConfigurationErrorsException ex)
            {
                success = false;
                msg = Localization.Save_ConnectionString_Exception + ex.Message;
                Trace.WriteLine(Localization.Save_ConnectionString_Exception + ex);
            }

            var icon = success ? MessageBoxIcon.Information : MessageBoxIcon.Warning;
            MessageBox.Show(msg, title, MessageBoxButtons.OK, icon);
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
                    // Trace.WriteLine(conn.ConnectionString);
                    conn.Open();
                    success = conn.State == ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            var title = ResourceMgr.GetString("TestConnTitle") ?? "Test Result";
            var msg = success ? Localization.Test_Connection_Success : Localization.Test_Connection_Fail;
            var icon = success ? MessageBoxIcon.Information : MessageBoxIcon.Warning;
            MessageBox.Show(msg, title, MessageBoxButtons.OK, icon);
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

        private void cboDataProvider_SelectedIndexChanged(object sender, EventArgs e)
        {
            var provider = cboDataProvider.Text;
            var port = DbProviderConfig.ToPort(provider);
            txtPort.Text = port == 0 ? string.Empty : port.ToString();
        }
    }
}
