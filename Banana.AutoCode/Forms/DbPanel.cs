using Banana.AutoCode.DbSchema;
using Banana.AutoCode.Core;
using Banana.AutoCode.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
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

        // 两个右键菜单的容器长期复用：菜单关闭过程中系统仍会回访该对象，
        // 在 Closed 里 Dispose 会抛 ObjectDisposedException。容器交给
        // 设计器组件容器，随 DbPanel.Dispose(bool) 的 components.Dispose() 释放。
        private ContextMenuStrip _connectionInfoMenu;
        private ContextMenuStrip _connectionStringMenu;

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
            tvDb.MouseDown += new MouseEventHandler(tvDb_MouseDown);
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

        /// <summary>
        /// 右键菜单按节点层级分流：
        /// 连接节点（根节点）只提供人读的「复制连接信息」；
        /// 数据库节点只提供九种格式的「复制连接字符串」；
        /// 表节点不提供菜单。
        /// 连接配置与库名都从节点自身取值，不依赖「当前选中连接」。
        /// </summary>
        void tvDb_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            var node = tvDb.GetNodeAt(e.X, e.Y);
            if (node == null)
            {
                return;
            }

            if (node.Level == ROOT_NODE_LEVEL)
            {
                var rootSettings = node.Tag as ConnectionStringSettings;
                if (rootSettings != null)
                {
                    ShowConnectionInfoMenu(rootSettings, e.Location);
                }

                return;
            }

            if (node.Level != DB_NODE_LEVEL)
            {
                return;
            }

            var settings = node.Parent == null ? null : node.Parent.Tag as ConnectionStringSettings;
            var db = node.Tag as Database;
            if (settings == null || db == null)
            {
                return;
            }

            ShowConnectionStringMenu(settings, db.Name, e.Location);
        }

        private void ShowConnectionStringMenu(ConnectionStringSettings settings, String databaseName, Point location)
        {
            // 无法识别的 Provider 不提供菜单，避免生成语义错误的结果。
            var model = CreateConnectionStringModel(settings, databaseName);
            if (model == null)
            {
                return;
            }

            var resolver = new ConnectionStringPlaceholderResolver(ResolveConnectionStringPlaceholder);

            // 只复用容器，菜单项每次按当前节点重建。
            var menu = GetConnectionStringMenu();
            ResetMenuItems(menu);

            var root = new ToolStripMenuItem(GetLocalizedText("ConnectionString_Menu_Copy", "Copy connection string"));
            menu.Items.Add(root);

            foreach (var format in DbConnectionStringFormats.AllFormats)
            {
                var item = new ToolStripMenuItem(DbConnectionStringFormats.GetFormatName(format));
                var result = DbConnectionStringFormats.Build(format, model, resolver);

                if (result == null)
                {
                    // 语义上不存在该组合（如 Go DSN 对 SQL Server），保持不可用。
                    item.Enabled = false;
                    item.ToolTipText = GetLocalizedText("ConnectionString_NotApplicable", "Not applicable to this provider");
                }
                else
                {
                    item.Tag = new ConnectionStringMenuContext
                    {
                        Settings = settings,
                        DatabaseName = databaseName,
                        Format = format
                    };

                    if (result.HasPlaceholders)
                    {
                        item.Text += String.Format(
                            GetLocalizedText("ConnectionString_MenuPlaceholderSuffix", " ({0} to fill in)"),
                            result.PlaceholderCount);
                    }

                    item.Click += new EventHandler(ConnectionStringMenuItem_Click);
                }

                root.DropDownItems.Add(item);
            }

            menu.Show(tvDb, location);
        }

        private void ConnectionStringMenuItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            var context = item == null ? null : item.Tag as ConnectionStringMenuContext;
            if (context == null)
            {
                return;
            }

            var format = context.Format;
            var formatName = DbConnectionStringFormats.GetFormatName(format);

            var model = CreateConnectionStringModel(context.Settings, context.DatabaseName);
            if (model == null)
            {
                ShowConnectionStringFeedback(String.Format(GetLocalizedText("ConnectionString_CopyFailed", "Copy failed: {0}"), formatName));
                return;
            }

            try
            {
                var resolver = new ConnectionStringPlaceholderResolver(ResolveConnectionStringPlaceholder);
                var result = DbConnectionStringFormats.Build(format, model, resolver);
                if (result == null)
                {
                    ShowConnectionStringFeedback(String.Format(GetLocalizedText("ConnectionString_CopyFailed", "Copy failed: {0}"), formatName));
                    return;
                }

                Clipboard.SetText(result.Text);

                // 反馈与日志只记格式名与待填写个数，不写入含密码的连接串。
                var message = result.HasPlaceholders
                    ? String.Format(
                        GetLocalizedText("ConnectionString_CopiedWithPlaceholders", "Copied: {0} ({1} item(s) to fill in before use)"),
                        formatName,
                        result.PlaceholderCount)
                    : String.Format(GetLocalizedText("ConnectionString_Copied", "Copied: {0}"), formatName);

                ShowConnectionStringFeedback(message);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                ShowConnectionStringFeedback(String.Format(GetLocalizedText("ConnectionString_CopyFailed", "Copy failed: {0}"), ex.Message));
            }
        }

        /// <summary>
        /// 连接节点（根节点）右键弹出「复制连接信息」菜单：
        /// 内容是给人阅读/转发的纯文本清单，与数据库节点上的连接字符串菜单互不影响。
        /// </summary>
        private void ShowConnectionInfoMenu(ConnectionStringSettings settings, Point location)
        {
            var model = DbProviderConfig.CreateDbViewModel(settings);
            if (model == null)
            {
                return;
            }

            var resolver = new ConnectionStringPlaceholderResolver(ResolveConnectionStringPlaceholder);

            // 只复用容器，菜单项每次按当前节点重建。
            var menu = GetConnectionInfoMenu();
            ResetMenuItems(menu);

            var root = new ToolStripMenuItem(GetLocalizedText("ConnectionInfo_Menu_Copy", "Copy connection info"));
            menu.Items.Add(root);

            AddConnectionInfoItem(root, settings, DbConnectionInfoPasswordMode.IncludePassword, "ConnectionInfo_Menu_IncludePassword", "Including password (plain text)", model, resolver);
            AddConnectionInfoItem(root, settings, DbConnectionInfoPasswordMode.ExcludePassword, "ConnectionInfo_Menu_ExcludePassword", "Without password", model, resolver);

            menu.Show(tvDb, location);
        }

        /// <summary>
        /// 连接信息菜单的复用容器，首次创建后加入设计器组件容器，
        /// 由 <c>Dispose(bool)</c> 里已有的 <c>components.Dispose()</c> 统一释放。
        /// </summary>
        private ContextMenuStrip GetConnectionInfoMenu()
        {
            if (_connectionInfoMenu == null)
            {
                _connectionInfoMenu = CreateReusableMenu();
            }

            return _connectionInfoMenu;
        }

        private ContextMenuStrip GetConnectionStringMenu()
        {
            if (_connectionStringMenu == null)
            {
                _connectionStringMenu = CreateReusableMenu();
            }

            return _connectionStringMenu;
        }

        private ContextMenuStrip CreateReusableMenu()
        {
            var menu = new ContextMenuStrip();
            menu.ShowItemToolTips = true;

            if (components != null)
            {
                components.Add(menu);
            }

            return menu;
        }

        /// <summary>
        /// 重建菜单项前清空旧项。<c>Items.Clear()</c> 不会释放菜单项句柄，
        /// 这里逐项移除并显式 Dispose（含子项），避免重复打开后句柄累积。
        /// </summary>
        private static void ResetMenuItems(ContextMenuStrip menu)
        {
            // 菜单还开着时不替换它正在跟踪的项。
            if (menu.Visible)
            {
                menu.Close();
            }

            DisposeMenuItems(menu.Items);
        }

        private static void DisposeMenuItems(ToolStripItemCollection items)
        {
            for (var i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];

                var menuItem = item as ToolStripMenuItem;
                if (menuItem != null)
                {
                    DisposeMenuItems(menuItem.DropDownItems);
                }

                items.RemoveAt(i);
                item.Dispose();
            }
        }

        private void AddConnectionInfoItem(
            ToolStripMenuItem parent,
            ConnectionStringSettings settings,
            DbConnectionInfoPasswordMode mode,
            String textKey,
            String textFallback,
            DbViewModel model,
            ConnectionStringPlaceholderResolver resolver)
        {
            var item = new ToolStripMenuItem(GetLocalizedText(textKey, textFallback));
            var result = DbConnectionInfoFormatter.Build(model, mode, resolver);

            if (result == null)
            {
                item.Enabled = false;
                item.ToolTipText = GetLocalizedText("ConnectionString_NotApplicable", "Not applicable to this provider");
                parent.DropDownItems.Add(item);
                return;
            }

            item.Tag = new ConnectionInfoMenuContext
            {
                Settings = settings,
                Mode = mode
            };

            if (result.HasPlaceholders)
            {
                item.Text += String.Format(
                    GetLocalizedText("ConnectionString_MenuPlaceholderSuffix", " ({0} to fill in)"),
                    result.PlaceholderCount);
            }

            item.Click += new EventHandler(ConnectionInfoMenuItem_Click);
            parent.DropDownItems.Add(item);
        }

        private void ConnectionInfoMenuItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            var context = item == null ? null : item.Tag as ConnectionInfoMenuContext;
            if (context == null)
            {
                return;
            }

            var model = DbProviderConfig.CreateDbViewModel(context.Settings);
            if (model == null)
            {
                ShowConnectionStringFeedback(GetLocalizedText("ConnectionString_NotApplicable", "Not applicable to this provider"));
                return;
            }

            try
            {
                var resolver = new ConnectionStringPlaceholderResolver(ResolveConnectionStringPlaceholder);
                var result = DbConnectionInfoFormatter.Build(model, context.Mode, resolver);
                if (result == null)
                {
                    ShowConnectionStringFeedback(GetLocalizedText("ConnectionString_NotApplicable", "Not applicable to this provider"));
                    return;
                }

                Clipboard.SetText(result.Text);

                var message = context.Mode == DbConnectionInfoPasswordMode.IncludePassword
                    ? GetLocalizedText("ConnectionInfo_CopiedWithPassword", "Copied connection info (contains plain-text password)")
                    : GetLocalizedText("ConnectionInfo_CopiedWithoutPassword", "Copied connection info (without password)");

                if (result.HasPlaceholders)
                {
                    message += String.Format(
                        GetLocalizedText("ConnectionString_MenuPlaceholderSuffix", " ({0} to fill in)"),
                        result.PlaceholderCount);
                }

                // 日志与提示只记模式，不写入清单内容（清单可能含明文密码）。
                ShowConnectionStringFeedback(message);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                ShowConnectionStringFeedback(String.Format(GetLocalizedText("ConnectionInfo_CopyFailed", "Copy failed: {0}"), ex.Message));
            }
        }

        /// <summary>
        /// 连接字符串按「连接配置 + 数据库节点」拼装。
        /// SQL Server / MySQL / PostgreSQL 的库名以节点为准；
        /// SQLite 的库标识是文件路径、Oracle 的库标识是 SERVICE_NAME，
        /// 节点名（"main" / schema 用户名）不代表库名，保留连接配置里的值。
        /// </summary>
        private static DbViewModel CreateConnectionStringModel(ConnectionStringSettings settings, String databaseName)
        {
            var model = DbProviderConfig.CreateDbViewModel(settings);
            if (model == null)
            {
                return null;
            }

            var provider = DbConnectionStringFormats.NormalizeProvider(model.Provider);
            if (provider == DbConnectionStringFormats.SqlServer
                || provider == DbConnectionStringFormats.MySql
                || provider == DbConnectionStringFormats.PostgreSQL)
            {
                model.Instance = databaseName;
            }

            return model;
        }

        /// <summary>
        /// 连接字符串菜单项上下文：命中时刻的连接配置、库名与格式快照，
        /// 避免依赖「当前选中连接」这类隐式状态。
        /// </summary>
        private sealed class ConnectionStringMenuContext
        {
            public ConnectionStringSettings Settings { get; set; }

            public String DatabaseName { get; set; }

            public DbConnectionFormat Format { get; set; }
        }

        /// <summary>
        /// 连接信息菜单项上下文。
        /// </summary>
        private sealed class ConnectionInfoMenuContext
        {
            public ConnectionStringSettings Settings { get; set; }

            public DbConnectionInfoPasswordMode Mode { get; set; }
        }

        /// <summary>
        /// 占位符文案跟随当前 UI 语言；键缺失时退化为键名，保证仍可识别。
        /// </summary>
        private static String ResolveConnectionStringPlaceholder(String key)
        {
            return GetLocalizedText(key, key);
        }

        /// <summary>
        /// 非阻塞反馈：状态栏同级的托盘式 ToolTip + 输出面板日志。
        /// </summary>
        private void ShowConnectionStringFeedback(String message)
        {
            Trace.WriteLine(message);

            try
            {
                var point = tvDb.PointToClient(Cursor.Position);
                toolTip.Show(message, tvDb, point.X + 12, point.Y + 18, 2500);
            }
            catch
            {
                // 提示失败不影响已经完成的复制动作。
            }
        }

        private static String GetLocalizedText(String key, String fallback)
        {
            var value = Localization.ResourceManager.GetString(key, Localization.Culture);
            return String.IsNullOrEmpty(value) ? fallback : value;
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
            // 连接配置绑定在根节点上：右键菜单直接从节点取，避免依赖「当前选中连接」。
            root.Tag = settings;
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
