using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Banana.AutoCode.Core;
using Banana.AutoCode.Forms;
using Microsoft.VisualStudio.TextTemplating;
using WeifenLuo.WinFormsUI.Docking;

namespace Banana.AutoCode
{
    public partial class Main : Form
    {
        const string TEMPLATES_DIR = "Templates";
        const string OUTPUT_DIR = "Output";

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

        private string GetOutputPath(Banana.AutoCode.DbSchema.Table table, string templateFile, string basePath)
        {
            var targetDir = Path.Combine(basePath, table.Owner
                , Path.GetDirectoryName(templateFile).Replace(TEMPLATES_DIR, string.Empty).Trim('\\')
                , Path.GetFileNameWithoutExtension(templateFile));

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            return targetDir;
        }

        private void runToolStripButton_Click(object sender, EventArgs e)
        {
            var tables = DbPanel.GetTables();

            if (tables == null || ! tables.Any())
            {
                Trace.WriteLine("Unchecked tables can not generate code");
                return;
            }

            Engine engine = new Engine();
            var host = new CustomHost();
            var files = Directory.EnumerateFiles(TEMPLATES_DIR, "*.tt", SearchOption.AllDirectories)
                .Concat(Directory.EnumerateFiles(TEMPLATES_DIR, "*.ttinclude", SearchOption.AllDirectories));

            var basePath = OUTPUT_DIR;
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            foreach (var path in files)
            {
                var content = File.ReadAllText(path);
                var templateName = Path.GetFileName(path);
                Trace.WriteLine("Template:" + templateName);
                
                foreach (var table in tables)
                {
                    host.TemplateFile = path;
                    host.Table = table;
                    
                    var outputPath = GetOutputPath(table, path, basePath);
                    host.SetValue("OutputPath", outputPath);

                    Trace.WriteLine("Generate table:" + host.Table.Name + " TemplateName:" + templateName + " OutputPath:" + outputPath);
                    var result = engine.ProcessTemplate(content, host);
                    
                    if (string.IsNullOrWhiteSpace(result))
                    {
                        Trace.WriteLine("Finish generate table " + host.Table.DisplayName + " code, no result.");
                        continue;
                    }

                    var targetPath = Path.Combine(outputPath, host.Table.DisplayName + host.FileExtension);

                    File.WriteAllText(targetPath, result);
                    Trace.WriteLine("Finish generate table " + host.Table.DisplayName + " code.");
                }
            }

            Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath));
        }

        private void optionsToolStripButton_Click(object sender, EventArgs e)
        {
            OptionsPanel options = new OptionsPanel();
            options.ShowDialog(this);            
        }

        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog(this);
        }
    }
}
