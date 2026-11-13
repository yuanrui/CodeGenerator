using Banana.AutoCode.Core;
using Banana.AutoCode.Forms;
using Banana.AutoCode.Resources;
using Mono.TextTemplating;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Banana.AutoCode
{
    public partial class Main : Form
    {
        public const string TEMPLATES_DIR = ConfigConstants.TEMPLATES_DIR;
        public const string OUTPUT_DIR = ConfigConstants.OUTPUT_DIR;

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

            //this.Text = this.Text + " - v" + About.VersionNumber;
            
            var theme = new VS2015LightTheme();
            this.dockPanel.Theme = theme;

            Trace.Listeners.Add(new OutputTraceListener(OutputPanel));
            OutputPanel.Show(this.dockPanel, DockState.DockBottom);

            // 关闭主窗口前把模板勾选的延迟保存落盘，避免状态丢失。
            this.FormClosing += new FormClosingEventHandler(Main_FormClosing);

            CheckDirectoryPath();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (TemplatePanel != null && !TemplatePanel.IsDisposed)
            {
                TemplatePanel.FlushPendingSelection();
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            // 启动时同时显示数据库面板（左侧）与模板面板（右侧），互不覆盖。
            if (DbPanel.IsDisposed)
            {
                DbPanel = new DbPanel();
            }

            DbPanel.Show(this.dockPanel, DockState.DockLeft);
            tsbtnDatabase.Checked = true;

            if (TemplatePanel == null || TemplatePanel.IsDisposed)
            {
                TemplatePanel = new TemplatePanel();
            }

            TemplatePanel.Show(this.dockPanel, DockState.DockRight);
            templateToolStripButton.Checked = true;
        }

        private void CheckDirectoryPath()
        {
            if (! Directory.Exists(TEMPLATES_DIR))
            {
                Trace.WriteLine(Localization.Templates_Not_Exists);

                Directory.CreateDirectory(TEMPLATES_DIR);
            }

            if (! Directory.Exists(OUTPUT_DIR))
            {
                Trace.WriteLine(Localization.Output_Not_Exists);
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
            saveFileDialog.Filter = Localization.SaveFileDialog_Filter;
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
                tsbtnDatabase.Checked = true;
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
            if (TemplatePanel == null || TemplatePanel.IsDisposed)
            {
                TemplatePanel = new TemplatePanel();
                TemplatePanel.Show(this.dockPanel);
                templateToolStripButton.Checked = true;
                return;
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
            var tables = DbPanel.GetTables();
            
            if (tables == null || ! tables.Any())
            {
                Trace.WriteLine(Localization.Unchecked_Tables);
                return;
            }

            // 只渲染被勾选的 .tt；.ttinclude 仅作为 include 被引用，不单独参与生成。
            var allTemplates = Directory.EnumerateFiles(TEMPLATES_DIR, "*.tt", SearchOption.AllDirectories).ToList();
            var files = new List<String>();

            foreach (var path in allTemplates)
            {
                var relativePath = TemplateSelection.GetRelativePath(TEMPLATES_DIR, path);

                if (TemplateSelection.IsEnabled(relativePath))
                {
                    files.Add(path);
                }
            }

            if (files.Count == 0)
            {
                Trace.WriteLine(Localization.No_Template_File);
                return;
            }

            Task.Factory.StartNew(() => {
                try
                {
                    DoRun(tables, files);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
            });
        }

        private void DoRun(IEnumerable<DbSchema.Table> tables, IEnumerable<string> files)
        {
            const string FILE_NAME_KEY = "FILE_NAME";
            var engine = new Mono.TextTemplating.TemplatingEngine();
            var host = new CustomHost();
#if NET
            engine.UseInProcessCompiler();
            host.UseInProcessCompiler();
#endif
            var basePath = OUTPUT_DIR;

            var tableList = tables.ToList();
            var templateList = files.ToList();
            var failures = new List<KeyValuePair<String, String>>();
            var succeeded = 0;

            Trace.WriteLine(String.Format(Localization.Templates_Selected_Count, templateList.Count));

            for (var index = 0; index < templateList.Count; index++)
            {
                var path = templateList[index];
                var relativePath = TemplateSelection.GetRelativePath(TEMPLATES_DIR, path).Replace('\\', '/');

                Trace.WriteLine(String.Format(Localization.Template_Progress, index + 1, templateList.Count, relativePath));

                try
                {
                    var content = File.ReadAllText(path);
                    var templateName = Path.GetFileName(path);
                    var errors = new List<String>();

                    foreach (var table in tableList)
                    {
                        host.TemplateFile = path;
                        host.Table = table;
                        host.ClearErrors();

                        var outputPath = GetOutputPath(table, basePath);
                        host.SetValue("OutputPath", outputPath);

                        Trace.WriteLine(String.Format(Localization.Generate_Table, host.Table.Name, templateName, outputPath));
                        var result = engine.ProcessTemplate(content, host);

                        // 模板解析/编译/运行时错误由引擎交给 host，不会以异常形式抛出，需要主动检查。
                        if (host.Errors.Count > 0)
                        {
                            for (var errorIndex = 0; errorIndex < host.Errors.Count; errorIndex++)
                            {
                                errors.Add(String.Format("{0}: {1}", table.Name, FormatErrorReason(host.Errors[errorIndex].ErrorText)));
                            }

                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(result))
                        {
                            Trace.WriteLine(String.Format(Localization.Finish_Generate_Empty, host.Table.DisplayName));
                            continue;
                        }

                        Object fileNameValue;
                        var fileName = host.TryGetValue(FILE_NAME_KEY, out fileNameValue) ? fileNameValue as String : null;

                        if (string.IsNullOrWhiteSpace(fileName))
                        {
                            fileName = host.Table.DisplayName;
                        }
                        else
                        {
                            host.SetValue(FILE_NAME_KEY, null);
                        }

                        var targetPath = Path.Combine(outputPath, fileName + host.FileExtension);

                        File.WriteAllText(targetPath, result, new UTF8Encoding(true));
                        Trace.WriteLine(String.Format(Localization.Finish_Generate, host.Table.DisplayName));
                    }

                    if (errors.Count > 0)
                    {
                        failures.Add(new KeyValuePair<String, String>(relativePath, String.Join("; ", errors.ToArray())));
                    }
                    else
                    {
                        succeeded++;
                    }
                }
                catch (Exception ex)
                {
                    failures.Add(new KeyValuePair<String, String>(relativePath, FormatErrorReason(ex.Message)));
                }
            }

            Trace.WriteLine(String.Format(Localization.Generate_Summary, succeeded, failures.Count));

            foreach (var failure in failures)
            {
                Trace.WriteLine(String.Format(Localization.Generate_Failure_Item, failure.Key, failure.Value));
            }

            var outputBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath);
            //BuildThriftCodeAsync(outputBasePath);
        }

        private static String FormatErrorReason(String error)
        {
            if (String.IsNullOrEmpty(error))
            {
                return String.Empty;
            }

            var text = error.Replace("\r", " ").Replace("\n", " ");

            while (text.IndexOf("  ", StringComparison.Ordinal) >= 0)
            {
                text = text.Replace("  ", " ");
            }

            text = text.Trim();

            return text.Length > 300 ? text.Substring(0, 300) + "..." : text;
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
            
            Trace.WriteLine(Localization.Generate_Thrift_Begin);

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
                        Trace.WriteLine(string.Format(Localization.Generate_Thrift_Exception, path, ex));
                    }                   
                }
            });

            task.ContinueWith(t => 
            {
                Trace.WriteLine(Localization.Generate_Thrift_Finished);
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
