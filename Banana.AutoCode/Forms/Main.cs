using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            
            var theme = new VS2015LightTheme();
            this.dockPanel.Theme = theme;

            Trace.Listeners.Add(new OutputTraceListener(OutputPanel));
            OutputPanel.Show(this.dockPanel, DockState.DockBottom);

            CheckDirectoryPath();
            //this.SetLanguage("en-US");
        }

        private void CheckDirectoryPath()
        {
            if (! Directory.Exists(TEMPLATES_DIR))
            {
                Trace.WriteLine("Templates directory not exists, auto create.");

                Directory.CreateDirectory(TEMPLATES_DIR);
            }

            if (! Directory.Exists(OUTPUT_DIR))
            {
                Trace.WriteLine("Output directory not exists, auto create.");
                Directory.CreateDirectory(OUTPUT_DIR);
            }
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

        private string GetOutputPath(Banana.AutoCode.DbSchema.Table table, string basePath)
        {
            var targetDir = Path.Combine(basePath, table.Owner);

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            return targetDir;
        }
        
        private void runToolStripButton_Click(object sender, EventArgs e)
        {
            const string FILE_NAME_KEY = "FILE_NAME";
            var tables = DbPanel.GetTables();
            
            if (tables == null || ! tables.Any())
            {
                Trace.WriteLine("Unchecked tables can not generate code");
                return;
            }

            var engine = new Engine();
            var host = new CustomHost();
            var files = Directory.EnumerateFiles(TEMPLATES_DIR, "*.tt", SearchOption.AllDirectories)
                .Concat(Directory.EnumerateFiles(TEMPLATES_DIR, "*.ttinclude", SearchOption.AllDirectories));
            
            if (! files.Any())
            {
                Trace.WriteLine("No template file. run stop.");
                return;
            }

            var basePath = OUTPUT_DIR;

            foreach (var path in files)
            {
                var content = File.ReadAllText(path);
                var templateName = Path.GetFileName(path);
                Trace.WriteLine("Template:" + templateName);

                foreach (var table in tables)
                {
                    host.TemplateFile = path;
                    host.Table = table;

                    var outputPath = GetOutputPath(table, basePath);
                    host.SetValue("OutputPath", outputPath);

                    Trace.WriteLine("Generate table:" + host.Table.Name + " TemplateName:" + templateName + " OutputPath:" + outputPath);
                    var result = engine.ProcessTemplate(content, host);
                    
                    if (string.IsNullOrWhiteSpace(result))
                    {
                        Trace.WriteLine("Finish generate table " + host.Table.DisplayName + " code, no result.");
                        continue;
                    }

                    var fileName = host.GetValue(FILE_NAME_KEY) as String;

                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        fileName = host.Table.DisplayName;
                    }
                    else
                    {
                        host.SetValue(FILE_NAME_KEY, null);
                    }

                    var targetPath = Path.Combine(outputPath, fileName + host.FileExtension);

                    File.WriteAllText(targetPath, result);
                    Trace.WriteLine("Finish generate table " + host.Table.DisplayName + " code.");
                }
            }

            var outputBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath);
            
            BuildThriftCodeAsync(outputBasePath);            
        }

        private void DoCommand(string thriftPath, string cmdText, string codePath)
        {
            Process process = Process.Start(new ProcessStartInfo(thriftPath, cmdText)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                WorkingDirectory = Path.GetDirectoryName(codePath)
            });

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                {
                    return;
                }

                Trace.WriteLine(e.Data);
            };

            process.ErrorDataReceived += delegate(object o, DataReceivedEventArgs args)
            {
                if (args.Data == null)
                {
                    return;
                }

                Trace.WriteLine(args.Data);
            };
        }

        private void DoGenerateCode(string codePath)
        {
            var thriftPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Thrift");
            var includePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Thrift");
            var thriftExe = Path.Combine(thriftPath, "thrift.exe");
            if (!File.Exists(thriftExe))
            {
                return;
            }

            var languages = new string[] { "java", "csharp", "js", "js:ts", "js:node", "py", "cpp" };

            foreach (var lang in languages)
            {
                var cmdText = "-r -I \"" + includePath + "\" --gen " + lang + " \"" + codePath + "\"";

                DoCommand(thriftExe, cmdText, codePath);
            }            
        }

        private void BuildThriftCodeAsync(string basePath)
        {
            var files = Directory.EnumerateFiles(basePath, "*.thrift", SearchOption.AllDirectories);

            if (files == null)
            {
                return;
            }
            
            Trace.WriteLine("Begin generate Thrift");

            var task = Task.Factory.StartNew(() =>
            {
                foreach (var path in files)
                {
                    Trace.Write(path);
                    try
                    {
                        DoGenerateCode(path);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(string.Format("Generate Thrift({0}) Exception:{1}", path, ex));
                    }                   
                }
            });

            task.ContinueWith(t => 
            {
                Trace.WriteLine("Finish generate Thrift");
            });
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

        private void buildToolStripButton_Click(object sender, EventArgs e)
        {
            var outputBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OUTPUT_DIR);
            BuildThriftCodeAsync(outputBasePath);
        }

    }
}
