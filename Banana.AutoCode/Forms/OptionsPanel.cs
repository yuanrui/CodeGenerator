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
using Banana.AutoCode.Resources;

namespace Banana.AutoCode.Forms
{
    public partial class OptionsPanel : Form
    {
        List<KeyValuePair<String, String>> LangSources = new List<KeyValuePair<String, String>>() 
        { 
            new KeyValuePair<String, String>("中文", "zh-CN"),
            new KeyValuePair<String, String>("English", "en-US")
        };

        public OptionsPanel()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            txtRemovePrefix.Text = ConfigurationManager.AppSettings["removePrefix"];
            txtRemoveSuffix.Text = ConfigurationManager.AppSettings["removeSuffix"];
            var culture = ConfigurationManager.AppSettings["culture"];

            cmbLanguage.DataSource = LangSources;
            cmbLanguage.DisplayMember = "Key";
            cmbLanguage.ValueMember = "Value";

            if (!CultureHelper.CultureIsSupport(culture))
            {
                culture = String.Empty;
            }

            cmbLanguage.SelectedValue = culture;
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

                Trace.WriteLine(String.Format(Localization.Save_Setting_Success, key, value));
            }
            catch (ConfigurationErrorsException)
            {
                Trace.WriteLine(Localization.Save_Setting_Exception);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            AddOrUpdateAppSettings("removePrefix", txtRemovePrefix.Text);
            AddOrUpdateAppSettings("removeSuffix", txtRemoveSuffix.Text);
            AddOrUpdateAppSettings("culture", ((String)cmbLanguage.SelectedValue ?? String.Empty));
        }
    }
}
