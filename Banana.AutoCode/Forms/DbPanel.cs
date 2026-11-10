using Banana.AutoCode.DbSchema;
using Banana.AutoCode.Core;
using Banana.AutoCode.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Banana.AutoCode.Forms
{
    public partial class DbPanel : DockContent, IDisposable
    {
        const Int32 ROOT_NODE_LEVEL = 0;
        const Int32 DB_NODE_LEVEL = 1;
        const Int32 TABLE_NODE_LEVEL = 2;

        const String DB_ON_ICON = "database";
        const String DB_OFF_ICON = "databaseOff";
        const String TABLE_OFF_ICON = "tables";

        protected ImageList IconList;
        protected ConnectionStringSettings CurrentConnSetting;

        protected TreeNode LastDbNode;

        protected Hashtable ManagerMap;
        protected Hashtable TableMap;
        protected ResourceManager ResourceMgr = new ResourceManager(typeof(DbPanel));

        public string DatabaseName { get; private set; }

        public DbPanel()
        {
            InitializeComponent();
            Init();
        }

        private static ImageList _sharedIconList;

        /// <summary>
        /// 连接树与各下拉共用的 16×16 图标列表，进程内只创建一次；
        /// 位图本身来自 <see cref="DbProviderIcons"/> 的静态缓存。
        /// </summary>
        internal static ImageList SharedIconList
        {
            get
            {
                if (_sharedIconList == null)
                {
                    var imageList = new ImageList();
                    imageList.ColorDepth = ColorDepth.Depth32Bit;

                    imageList.Images.Add(DbProviderIcons.DefaultKey, DbProviderIcons.DefaultImage);
                    imageList.Images.Add(DB_ON_ICON, Properties.Resources.Icons_16x16_DataActive);
                    imageList.Images.Add(DB_OFF_ICON, Properties.Resources.Icons_16x16_DataInactive);
                    imageList.Images.Add(TABLE_OFF_ICON, Properties.Resources.Icons_16x16_Table);

                    foreach (var icon in DbProviderIcons.Images)
                    {
                        imageList.Images.Add(icon.Key, icon.Value);
                    }

                    _sharedIconList = imageList;
                }

                return _sharedIconList;
            }
        }

        protected void Init()
        {
            ManagerMap = new Hashtable();
            TableMap = new Hashtable();
            IconList = SharedIconList;

            tvDb.ImageList = IconList;
            tvDb.DrawMode = TreeViewDrawMode.OwnerDrawText;
            tvDb.DrawNode += new DrawTreeNodeEventHandler(tvDb_DrawNode);
            tvDb.NodeMouseClick += new TreeNodeMouseClickEventHandler(tvDb_NodeMouseClick);
            tvDb.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(tvDb_NodeMouseDoubleClick);
            toolTip.SetToolTip(this.pboxDbAdd, ResourceMgr.GetString("DbAddToolTip"));
            toolTip.SetToolTip(this.pboxDbEdit, ResourceMgr.GetString("DbEditToolTip"));
            toolTip.SetToolTip(this.pboxDbDelete, ResourceMgr.GetString("DbDeleteToolTip"));
            toolTip.SetToolTip(this.pboxDbRefresh, ResourceMgr.GetString("DbRefreshToolTip"));

            // 可编辑 ComboBox 的编辑区由原生 EDIT 绘制，不会触发 DrawItem，
            // 子控件覆盖也会被原生编辑框盖住；改为 DropDownList 后收起状态
            // 以 ComboBoxEdit 状态回到同一套 DrawItem，图标才能稳定显示。
            cbConnStrings.DropDownStyle = ComboBoxStyle.DropDownList;
            DbIconComboBox.Apply(cbConnStrings);
            cbConnStrings.DrawItem += new DrawItemEventHandler(cbConnStrings_DrawItem);

            RefreshTreeView();
        }

        public void InitConnStrings()
        {
            cbConnStrings.Items.Clear();

            ConfigurationManager.RefreshSection("connectionStrings");
            foreach (ConnectionStringSettings css in ConfigurationManager.ConnectionStrings)
            {
                if (String.IsNullOrWhiteSpace(css.ConnectionString) || String.IsNullOrWhiteSpace(css.ProviderName))
                {
                    continue;
                }

                cbConnStrings.Items.Add(css.Name);
            }

            DbIconComboBox.FitDropDownWidth(cbConnStrings);
        }

        protected void RefreshTreeView()
        {
            InitConnStrings();
            tvDb.Nodes.Clear();

            foreach (DictionaryEntry item in TableMap)
            {
                var form = item.Value as TablePanel;
                if (form == null)
                {
                    continue;
                }

                form.Close();
            }

            TableMap.Clear();
            //cbConnStrings.Text = String.Empty;

        }

        private void ResetDbNodeIcon(TreeNode dbNode)
        {
            if (dbNode == null || dbNode == LastDbNode)
            {
                return;
            }

            if (LastDbNode != null)
            {
                LastDbNode.ImageKey = DB_OFF_ICON;
                LastDbNode.SelectedImageKey = DB_OFF_ICON;
            }
            LastDbNode = dbNode;
            LastDbNode.ImageKey = DB_ON_ICON;
            LastDbNode.SelectedImageKey = DB_ON_ICON;
        }

        void tvDb_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var table = e.Node.Tag as Table;

            if (e.Node.Level == DB_NODE_LEVEL)
            {
                DatabaseName = e.Node.Text;
                ResetDbNodeIcon(e.Node);
            }

            if (e.Node.Level == TABLE_NODE_LEVEL)
            {
                ResetDbNodeIcon(e.Node.Parent);

                if (table == null)
                {
                    return;
                }

                var tableMapKey = cbConnStrings.Text + "." + table.ToString();
                var tableForm = TableMap[tableMapKey] as TablePanel;
                if (tableForm == null)
                {
                    tableForm = new TablePanel(table, CurrentConnSetting);
                    TableMap[tableMapKey] = tableForm;
                    tableForm.Text = e.Node.Text;
                    tableForm.Show(((Main)this.ParentForm).MainDockPanel);
                }
                else
                {
                    if (tableForm.IsDisposed)
                    {
                        tableForm = new TablePanel(table, CurrentConnSetting);
                        tableForm.Text = e.Node.Text;
                        tableForm.Show(((Main)this.ParentForm).MainDockPanel);

                        return;
                    }

                    tableForm.Activate();
                }
            }
        }

        void tvDb_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level != DB_NODE_LEVEL)
            {
                return;
            }

            var dbNode = e.Node;

            var db = dbNode.Tag as Database;
            if (db == null)
            {
                return;
            }

            DatabaseName = db.Name;

            if (db.Tables == null)
            {
                var tables = GetManager().GetTables(db);
                db.Tables = tables;

                foreach (var table in tables)
                {
                    var tableNode = new TreeNode(table.Name, 2, 2);
                    tableNode.Tag = table;
                    tableNode.ToolTipText = string.IsNullOrWhiteSpace(table.Comment) ? table.Name : table.Comment;
                    tableNode.ImageKey = TABLE_OFF_ICON;
                    tableNode.SelectedImageKey = tableNode.ImageKey;
                    dbNode.Nodes.Add(tableNode);
                }
            }

            e.Node.Toggle();
        }

        void tvDb_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node.Level == ROOT_NODE_LEVEL || e.Node.Level == DB_NODE_LEVEL)
            {
                e.Node.HideCheckBox();
            }

            e.DrawDefault = true;
        }

        private void cbConnStrings_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Items 被清空（刷新配置）时选中项为 null，这里直接返回，避免空引用。
            if (cbConnStrings.SelectedItem == null)
            {
                return;
            }

            tvDb.Nodes.Clear();

            CurrentConnSetting = ConfigurationManager.ConnectionStrings[cbConnStrings.SelectedItem.ToString()];

            Task.Factory.StartNew(() => {
                DoChangeDataSource(CurrentConnSetting);
            });
        }

        private void cbConnStrings_DrawItem(object sender, DrawItemEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null)
            {
                return;
            }

            var text = e.Index >= 0 && e.Index < combo.Items.Count
                ? combo.GetItemText(combo.Items[e.Index])
                : combo.Text;

            // 设计器里不解析配置，降级为纯文本绘制，避免设计时异常。
            if (this.DesignMode || combo.Items.Count == 0)
            {
                DbIconComboBox.DrawPlain(e, combo, text);
                return;
            }

            DbIconComboBox.Draw(e, combo, text, GetProviderName(text));
        }

        /// <summary>
        /// 按连接名反查配置里的 ProviderName；取不到时返回空串，由 <see cref="DbProviderIcons.GetImage"/>
        /// 回退到默认数据库图标。
        /// </summary>
        private static string GetProviderName(string connectionName)
        {
            if (String.IsNullOrWhiteSpace(connectionName))
            {
                return String.Empty;
            }

            var settings = ConfigurationManager.ConnectionStrings[connectionName];
            return settings == null || settings.ProviderName == null ? String.Empty : settings.ProviderName;
        }

        private void DoChangeDataSource(ConnectionStringSettings settings)
        {
            var dbSchemaManager = GetManager();

            var dbs = dbSchemaManager.GetDatabases();
            var root = new TreeNode(settings.Name, 0, 0);
            root.ToolTipText = settings.ConnectionString;
            root.ImageKey = DbProviderIcons.GetIconKey(settings.ProviderName);
            root.SelectedImageKey = root.ImageKey;

            foreach (var db in dbs)
            {
                var dbNode = new TreeNode(db.Name, 1, 1);
                dbNode.Tag = db;
                dbNode.ToolTipText = db.Name;
                dbNode.ImageKey = DB_OFF_ICON;
                dbNode.SelectedImageKey = dbNode.ImageKey;

                root.Nodes.Add(dbNode);
            }

            tvDb.InvokeIfRequired((tv) => {
                tv.Nodes.Add(root);
                tv.CheckBoxes = true;
            });
        }

        private DbSchemaManager GetManager()
        {
            var dbSchemaManager = ManagerMap[CurrentConnSetting] as DbSchemaManager;
            if (dbSchemaManager == null)
            {
                dbSchemaManager = new DbSchemaManager(CurrentConnSetting);
                ManagerMap[CurrentConnSetting] = dbSchemaManager;
            }

            return dbSchemaManager;
        }

        public List<Table> GetTables()
        {
            var result = new List<Table>();
            var nodes = tvDb.Nodes;

            foreach (TreeNode rootNode in nodes)
            {
                foreach (TreeNode dbNode in rootNode.Nodes)
                {
                    if (dbNode.Text != DatabaseName)
                    {
                        continue;
                    }

                    foreach (TreeNode tableNode in dbNode.Nodes)
                    {
                        if (tableNode.Checked)
                        {
                            var table = tableNode.Tag as Table;
                            if (table != null)
                            {
                                result.Add(table);
                            }                            
                        }
                    }
                }
            }

            return result;
        }

        void IDisposable.Dispose()
        {
            this.ManagerMap.Clear();
            this.ManagerMap = null;

            this.TableMap.Clear();
            this.TableMap = null;

            this.Dispose(true);
        }

        public override void Refresh()
        {
            base.Refresh();
            RefreshTreeView();
        }

        private void pboxDbAdd_Click(object sender, EventArgs e)
        {
            DbConnBuilderPanel options = new DbConnBuilderPanel(this);
            options.ShowDialog(this);
        }

        private void pboxDbEdit_Click(object sender, EventArgs e)
        {
            DbConnBuilderPanel options = new DbConnBuilderPanel(this, CurrentConnSetting);
            options.ShowDialog(this);
        }

        private void pboxDbDelete_Click(object sender, EventArgs e)
        {
            var name = cbConnStrings.Text;
            var msg = string.Format(Localization.Remove_ConnectionString_Confirm, name);
            var dlgResult = MessageBox.Show(msg, Localization.Caption_Prompt, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

            if (dlgResult != DialogResult.OK)
            {
                return;
            }

            RemoveConnectionStrings(name);
            RefreshTreeView();
        }

        private void pboxDbRefresh_Click(object sender, EventArgs e)
        {
            RefreshTreeView();
        }

        public void RemoveConnectionStrings(string name)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var collection = configFile.ConnectionStrings.ConnectionStrings;
                collection.Remove(name);
                cbConnStrings.Items.Remove(name);
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.ConnectionStrings.SectionInformation.Name);

                Trace.WriteLine(String.Format(Localization.Remove_ConnectionString_Success, name));
            }
            catch (ConfigurationErrorsException ex)
            {
                Trace.WriteLine(Localization.Remove_ConnectionString_Exception + ex);
            }
        }

    }
}
