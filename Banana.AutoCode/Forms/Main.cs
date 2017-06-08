using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Banana.AutoCode.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Microsoft.VisualStudio.TextTemplating;
using Banana.AutoCode.Core;
using System.IO;

namespace Banana.AutoCode
{
    public partial class Main : Form
    {
        private int childFormNumber = 0;
        DbPanel DbPanel = new DbPanel();
        OutputPanel OutputPanel = new OutputPanel();
        TemplatePanel TemplatePanel = null;

        public DockPanel MainDockPanel 
        { 
            get 
            {
                return this.dockPanel;
            }
        }

        public Main()
        {
            InitializeComponent();
            Init();
        }

        protected void Init()
        {
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            this.Text = this.Text + " - v" + About.VersionNumber;
            Trace.Listeners.Add(new OutputTraceListener(OutputPanel));
            OutputPanel.Show(this.dockPanel, DockState.DockBottom);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Trace.TraceError(e.ExceptionObject.ToString());
        }

        void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Trace.TraceError(e.Exception.ToString());
        }

        private void ShowNewForm(object sender, EventArgs e)
        {
            //var childForm = new TablePanel(null);
            //childForm.MdiParent = this;
            //childForm.Text = "窗口 " + childFormNumber++;
            //childForm.Show(this.dockPanel);
            ////childForm.DockTo(this.dockPanel);
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

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            
        }

        private void templateToolStripButton_Click(object sender, EventArgs e)
        {
            if (TemplatePanel == null)
            {
                TemplatePanel = new TemplatePanel();
            }
            
            if (templateToolStripButton.Checked)
            {
                TemplatePanel.Hide();
            }
            else
            {
                TemplatePanel.Show(this.dockPanel);
            }

            templateToolStripButton.Checked = !templateToolStripButton.Checked;
        }

        private void reloadToolStripButton_Click(object sender, EventArgs e)
        {
            if (TemplatePanel != null)
            {
                TemplatePanel.Refresh();
            }

            DbPanel.Refresh();
        }

        private void runToolStripButton_Click(object sender, EventArgs e)
        {
            Engine engine = new Engine();
            var host = new CustomHost();
            var files = Directory.EnumerateFiles("Templates", "*.tt", SearchOption.AllDirectories);
            var basePath = "Output";
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            foreach (var path in files)
            {
                var content = File.ReadAllText(path);
                host.TemplateFile = path;
                var result = engine.ProcessTemplate(content, host);

                File.WriteAllText(Path.Combine(basePath, Path.GetFileNameWithoutExtension(path) + ".cs"), result);
            }
        }
    }
}
