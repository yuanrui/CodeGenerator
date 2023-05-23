using Banana.AutoCode.Core;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Banana.AutoCode
{
    static class Program
    {
        static Program()
        {
#if !NET
            CosturaUtility.Initialize();
#endif
#if NET
            DbProviderFactories.RegisterFactory("System.Data.SQLite", SQLiteFactory.Instance);
            DbProviderFactories.RegisterFactory("MySql.Data.MySqlClient", MySqlClientFactory.Instance);
            DbProviderFactories.RegisterFactory("Oracle.ManagedDataAccess.Client", OracleClientFactory.Instance);
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);
#endif
        }

        [STAThread]
        static void Main()
        {
            SetEnvironment();
            CultureHelper.SetCulture();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }

        static void SetEnvironment()
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrEmpty(path))
            {
                path = AppDomain.CurrentDomain.BaseDirectory;
            }
            var directory = System.IO.Path.GetDirectoryName(path);
            Environment.CurrentDirectory = directory;
            Environment.SetEnvironmentVariable("APP_DIR", directory);
            Environment.SetEnvironmentVariable("OUTPUT_DIR", Path.Combine(directory, ConfigConstants.OUTPUT_DIR));
            Environment.SetEnvironmentVariable("TEMPLATES_DIR", Path.Combine(directory, ConfigConstants.TEMPLATES_DIR));
        }
    }
}
