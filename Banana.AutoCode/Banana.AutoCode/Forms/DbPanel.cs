using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Banana.AutoCode.Forms
{
    public partial class DbPanel : DockContent
    {
        public DbPanel()
        {
            InitializeComponent();
            Init();
        }

        protected void Init()
        {
            cbConnectionStrings.Items.Clear();
            chkListTables.Items.Clear();

            foreach (ConnectionStringSettings css in ConfigurationManager.ConnectionStrings)
            {
                cbConnectionStrings.Items.Add(css.Name);
            }
        }
    }
}
