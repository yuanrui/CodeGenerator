using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Banana.AutoCode.Forms
{
    public partial class OptionsPanel : Form
    {
        public OptionsPanel()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            txtRemovePrefix.Text = ConfigurationManager.AppSettings["removePrefix"];
            txtRemoveSuffix.Text = ConfigurationManager.AppSettings["removeSuffix"];
        }

        public static void AddOrUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);

                Trace.WriteLine("Save settings " + key + "=" + value + " success");                
            }
            catch (ConfigurationErrorsException)
            {
                Trace.WriteLine("Error writing app settings");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            AddOrUpdateAppSettings("removePrefix", txtRemovePrefix.Text);
            AddOrUpdateAppSettings("removeSuffix", txtRemoveSuffix.Text);
            //MessageBox.Show("Save settings success");
        }
    }
}
