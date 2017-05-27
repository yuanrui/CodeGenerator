using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Banana.AutoCode.Forms;

namespace Banana.AutoCode
{
    public partial class Main : Form
    {
        private int childFormNumber = 0;
        DbPanel DbPanel = new DbPanel();

        public Main()
        {
            InitializeComponent();
            Init();
        }

        protected void Init()
        {
            this.Text = this.Text + " - v" + About.VersionNumber;
        }

        private void ShowNewForm(object sender, EventArgs e)
        {
            //var childForm = new DbPanel();
            //childForm.MdiParent = this;
            //childForm.Text = "窗口 " + childFormNumber++;
            //childForm.Show(this.dockPanel);
            //childForm.DockTo(this.dockPanel, DockStyle.Left);
        }

        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            openFileDialog.Filter = "文本文件(*.txt)|*.txt|所有文件(*.*)|*.*";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = openFileDialog.FileName;
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            saveFileDialog.Filter = "文本文件(*.txt)|*.txt|所有文件(*.*)|*.*";
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = saveFileDialog.FileName;
            }
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void ToolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip.Visible = toolBarToolStripMenuItem.Checked;
        }

        private void StatusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStrip.Visible = statusBarToolStripMenuItem.Checked;
        }

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About f = new About();
            f.ShowDialog();
            f.Dispose();
        }

        private void tsbtnDatabase_Click(object sender, EventArgs e)
        {
            if (DbPanel.IsDisposed)
            {
                DbPanel = new DbPanel();
                DbPanel.Show(this.dockPanel);
                tsbtnDatabase.Checked = !tsbtnDatabase.Checked;
                return;
            }

            if (tsbtnDatabase.Checked)
            {
                DbPanel.Hide();
            }
            else
            {
                DbPanel.Show(this.dockPanel);
            }
            tsbtnDatabase.Checked = ! tsbtnDatabase.Checked;
        }
    }
}
