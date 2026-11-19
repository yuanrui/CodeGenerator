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

        // 在分级刷新中静默调整 ComboBox 选中项时，临时屏蔽
        // SelectedIndexChanged，避免触发全量树重建覆盖局部刷新结果。
        private bool _suppressConnectionChanged;

        private void SuppressConnectionChanged(Action action)
        {
            var prev = _suppressConnectionChanged;
            _suppressConnectionChanged = true;
            try
            {
                action();
            }
            finally
            {
                _suppressConnectionChanged = prev;
            }
        }

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
            // 记住当前选中的连接名，刷新配置后需要恢复。
            var selectedName = cbConnStrings.SelectedItem != null
                ? cbConnStrings.SelectedItem.ToString()
                : null;

            InitConnStrings();

            // 关闭已打开的表面板并清空映射。
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

            // 恢复之前选中的连接；如果该连接已被删除，则保持未选中状态。
            if (selectedName != null)
            {
                var index = cbConnStrings.Items.IndexOf(selectedName);
                if (index >= 0)
                {
                    // 先清空树（InitConnStrings 里 Clear Items 已触发过一次事件，
                    // 但 SelectedIndex 当时是 -1，tvDb 还没被清空；这里显式清空）。
                    tvDb.Nodes.Clear();

                    // 清空 ManagerMap，确保刷新时用最新配置重建 DbSchemaManager。
                    // 键是 ConnectionStringSettings 对象实例，配置刷新后旧键
                    // 可能已失效，整体清空最稳妥。
                    ManagerMap.Clear();

                    cbConnStrings.SelectedIndex = index;
                    return;
                }
            }

            // 没有可恢复的选中项，只清空树。
            tvDb.Nodes.Clear();
            CurrentConnSetting = null;
            DatabaseName = null;
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

            // 连接参数取连接配置本身；库名用数据库节点自身（库节点名可能比配置里的更新）。
            var infoModel = DbProviderConfig.CreateDbViewModel(settings);

            var resolver = new ConnectionStringPlaceholderResolver(ResolveConnectionStringPlaceholder);

            // 只复用容器，菜单项每次按当前节点重建。
            var menu = GetConnectionStringMenu();
            ResetMenuItems(menu);

            // 菜单顺序：复制连接参数 → 分隔线 → 复制连接字符串 ▸ 九种格式。
            if (infoModel != null)
            {
                menu.Items.Add(CreateConnectionInfoMenuItem(settings, infoModel, resolver, databaseName));
                menu.Items.Add(new ToolStripSeparator());
            }

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
        /// 连接节点（根节点）右键弹出「复制连接参数」。
        /// 与数据库节点上的同名项共用 <see cref="CreateConnectionInfoMenuItem"/>，
        /// 配置取自被右键的节点自身，不依赖「当前选中连接」。
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

            menu.Items.Add(CreateConnectionInfoMenuItem(settings, model, resolver, null));

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

        /// <summary>
        /// 生成「复制连接参数」菜单项。连接节点与数据库节点共用这一套生成逻辑：
        /// 配置由调用方从被右键的节点取得，输出固定包含密码（为空时用占位符）。
        /// <paramref name="databaseName"/> 为数据库节点自身的库名；连接节点传 null，
        /// 由连接配置里的库名回退。
        /// </summary>
        private ToolStripMenuItem CreateConnectionInfoMenuItem(
            ConnectionStringSettings settings,
            DbViewModel model,
            ConnectionStringPlaceholderResolver resolver,
            String databaseName)
        {
            var item = new ToolStripMenuItem(GetLocalizedText("ConnectionInfo_Menu_CopyParams", "Copy connection parameters"));
            var result = DbConnectionInfoFormatter.Build(model, databaseName, resolver);

            if (result == null)
            {
                item.Enabled = false;
                item.ToolTipText = GetLocalizedText("ConnectionString_NotApplicable", "Not applicable to this provider");
                return item;
            }

            item.Tag = new ConnectionInfoMenuContext
            {
                Settings = settings,
                DatabaseName = databaseName
            };

            if (result.HasPlaceholders)
            {
                item.Text += String.Format(
                    GetLocalizedText("ConnectionString_MenuPlaceholderSuffix", " ({0} to fill in)"),
                    result.PlaceholderCount);
            }

            item.Click += new EventHandler(ConnectionInfoMenuItem_Click);
            return item;
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
                var result = DbConnectionInfoFormatter.Build(model, context.DatabaseName, resolver);
                if (result == null)
                {
                    ShowConnectionStringFeedback(GetLocalizedText("ConnectionString_NotApplicable", "Not applicable to this provider"));
                    return;
                }

                Clipboard.SetText(result.Text);

                var message = GetLocalizedText("ConnectionInfo_Copied", "Copied connection parameters (contains plain-text password)");

                if (result.HasPlaceholders)
                {
                    message += String.Format(
                        GetLocalizedText("ConnectionString_MenuPlaceholderSuffix", " ({0} to fill in)"),
                        result.PlaceholderCount);
                }

                // 日志与提示只记结果类型，不写入清单内容（清单含明文密码）。
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
        /// 连接参数菜单项上下文：命中时刻的连接配置与库名快照。
        /// 数据库节点的库名取自节点自身，避免配置里的库名过期或不同。
        /// </summary>
        private sealed class ConnectionInfoMenuContext
        {
            public ConnectionStringSettings Settings { get; set; }

            public String DatabaseName { get; set; }
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
            // 分级刷新中静默调整 ComboBox 选中项时，不触发全量树重建。
            if (_suppressConnectionChanged)
            {
                return;
            }

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
            RefreshAtCurrentLevel();
        }

        /// <summary>
        /// 按当前 TreeView 选中节点的层级，执行分级刷新：
        /// - 未选中或选中连接根节点 → 刷新连接下拉列表 + 数据库列表，保留展开状态
        /// - 选中数据库节点 → 刷新该数据库下的表列表
        /// - 选中数据表节点 → 刷新该表对应的 TablePanel 字段内容
        /// 任何情况下不破坏已有展开/折叠状态。
        /// </summary>
        private void RefreshAtCurrentLevel()
        {
            var prevConnName = cbConnStrings.SelectedItem?.ToString();

            // 1. 始终先刷新 ComboBox 连接列表（纯配置读取，不碰树）
            InitConnStrings();

            // 2. 彻底清空 DbSchemaManager 缓存，让后续 GetManager() 都 new 干净实例。
            //    DbSchemaManager 是轻量对象（每次构造只 new Provider + 拿 Thread slot DataContext 引用），
            //    连接管理由内部 using (scope.Begin()) 保证 Open/Close 生命周期，
            //    缓存反而容易因为 Thread slot 里的 DataContext 状态残留导致 "Connection is not open"。
            ManagerMap.Clear();

            var selectedNode = tvDb.SelectedNode;

            // 3. 若连接已被删除，直接清空树并返回
            if (prevConnName != null)
            {
                var curIdx = cbConnStrings.Items.IndexOf(prevConnName);
                if (curIdx < 0)
                {
                    tvDb.Nodes.Clear();
                    CurrentConnSetting = null;
                    DatabaseName = null;
                    return;
                }
            }

            // 4. 恢复 ComboBox 选中项（静默，不触发 SelectedIndexChanged）
            if (prevConnName != null)
            {
                var curIdx = cbConnStrings.Items.IndexOf(prevConnName);
                if (curIdx >= 0 && curIdx != cbConnStrings.SelectedIndex)
                {
                    SuppressConnectionChanged(() =>
                    {
                        cbConnStrings.SelectedIndex = curIdx;
                    });
                }
            }

            // 5. 按当前选中层级，执行对应范围的刷新
            if (selectedNode == null || selectedNode.Level == ROOT_NODE_LEVEL)
            {
                RefreshConnectionLevel();
            }
            else if (selectedNode.Level == DB_NODE_LEVEL)
            {
                RefreshDatabaseLevel(selectedNode);
            }
            else if (selectedNode.Level == TABLE_NODE_LEVEL)
            {
                RefreshTableLevel(selectedNode);
            }
        }

        /// <summary>
        /// 连接级刷新：重建根节点下的全部数据库节点。
        /// 后台线程查库列表（避免 UI 卡死），UI 线程建树并恢复展开/勾选状态。
        /// </summary>
        private void RefreshConnectionLevel()
        {
            if (CurrentConnSetting == null)
            {
                return;
            }

            // ── UI 线程阶段：保存当前状态、清空旧面板 ──
            var settings = ConfigurationManager.ConnectionStrings[CurrentConnSetting.Name];
            if (settings == null)
            {
                CurrentConnSetting = null;
                tvDb.Nodes.Clear();
                return;
            }

            var expanded = SaveExpandedDbNames();
            var checkedTables = SaveCheckedTables();

            // 同步 ComboBox（静默）：确保 RefreshAtCurrentLevel 上层已设置好
            SuppressConnectionChanged(() =>
            {
                var idx = cbConnStrings.Items.IndexOf(settings.Name);
                if (idx >= 0 && cbConnStrings.SelectedIndex != idx)
                {
                    cbConnStrings.SelectedIndex = idx;
                }
            });

            // 关闭该连接下已打开的表面板
            CloseTablePanelsForConnection();

            // ── 后台线程阶段：查库列表、构造 TreeNode 结构 ──
            Task.Factory.StartNew(() =>
            {
                var manager = new DbSchemaManager(settings);
                var dbs = manager.GetDatabases();

                var root = new TreeNode(settings.Name, 0, 0);
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

                return root;
            }).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    return;
                }

                var root = t.Result;
                tvDb.InvokeIfRequired(tv =>
                {
                    CurrentConnSetting = settings;
                    tv.Nodes.Clear();
                    tv.Nodes.Add(root);
                    tv.CheckBoxes = true;

                    // 恢复展开状态和勾选状态
                    RestoreExpandedDbNames(expanded);
                    RestoreCheckedTables(checkedTables);
                });
            });
        }

        /// <summary>
        /// 数据库级刷新：重新查询指定数据库下的所有表，重建表节点。
        /// 后台线程查表列表，数据库节点本身保持原来的展开/折叠状态不变。
        /// </summary>
        private void RefreshDatabaseLevel(TreeNode dbNode)
        {
            var db = dbNode.Tag as Database;
            if (db == null || CurrentConnSetting == null)
            {
                return;
            }

            // ── UI 线程阶段：保存勾选状态 ──
            var checkedTables = new HashSet<string>();
            foreach (TreeNode tableNode in dbNode.Nodes)
            {
                if (tableNode.Checked)
                {
                    checkedTables.Add(tableNode.Text);
                }
            }

            var connSettings = CurrentConnSetting;

            // ── 后台线程阶段：重新查 Tables ──
            Task.Factory.StartNew(() =>
            {
                var manager = new DbSchemaManager(connSettings);
                return manager.GetTables(db);
            }).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    return;
                }

                var tables = t.Result;

                tvDb.InvokeIfRequired(_ =>
                {
                    // 同步回 Database 对象的 Tables 缓存
                    db.Tables = tables;

                    dbNode.Nodes.Clear();
                    foreach (var table in tables)
                    {
                        var tableNode = new TreeNode(table.Name, 2, 2);
                        tableNode.Tag = table;
                        tableNode.ToolTipText = string.IsNullOrWhiteSpace(table.Comment) ? table.Name : table.Comment;
                        tableNode.ImageKey = TABLE_OFF_ICON;
                        tableNode.SelectedImageKey = tableNode.ImageKey;
                        if (checkedTables.Contains(table.Name))
                        {
                            tableNode.Checked = true;
                        }

                        dbNode.Nodes.Add(tableNode);
                    }
                });
            });
        }

        /// <summary>
        /// 数据表级刷新：重新查询该表的字段结构，刷新已打开的 TablePanel 内容。
        /// 后台线程查字段，表节点本身保持在树中，不影响其他节点状态。
        /// </summary>
        private void RefreshTableLevel(TreeNode tableNode)
        {
            var table = tableNode.Tag as Table;
            if (table == null || CurrentConnSetting == null)
            {
                return;
            }

            var connSettings = CurrentConnSetting;

            // ── 后台线程阶段：重新查字段 ──
            Task.Factory.StartNew(() =>
            {
                var manager = new DbSchemaManager(connSettings);
                return manager.GetColumns(table);
            }).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    return;
                }

                var columns = t.Result;

                tvDb.InvokeIfRequired(_ =>
                {
                    // 更新 Table.Tag 里的缓存引用，让后续代码生成拿到最新字段
                    table.Columns = columns;

                    // 刷新已打开的 TablePanel（如果有）
                    var key = cbConnStrings.Text + "." + table.ToString();
                    var tableForm = TableMap[key] as TablePanel;
                    if (tableForm != null && !tableForm.IsDisposed)
                    {
                        tableForm.RefreshFields(columns);
                    }
                });
            });
        }

        // ──────────────────────────────────────────────────────────────────────
        // 展开/勾选状态保存与恢复辅助
        // ──────────────────────────────────────────────────────────────────────

        private HashSet<string> SaveExpandedDbNames()
        {
            var set = new HashSet<string>();
            foreach (TreeNode root in tvDb.Nodes)
            {
                foreach (TreeNode dbNode in root.Nodes)
                {
                    if (dbNode.IsExpanded)
                    {
                        set.Add(dbNode.Text);
                    }
                }
            }

            return set;
        }

        private void RestoreExpandedDbNames(HashSet<string> expanded)
        {
            foreach (TreeNode root in tvDb.Nodes)
            {
                foreach (TreeNode dbNode in root.Nodes)
                {
                    if (expanded.Contains(dbNode.Text))
                    {
                        dbNode.Expand();
                    }
                }
            }
        }

        /// <summary>
        /// 保存当前树中所有表节点的勾选状态。键为数据库名称，值为该数据库下被勾选的表名集合。
        /// </summary>
        private Dictionary<string, HashSet<string>> SaveCheckedTables()
        {
            var result = new Dictionary<string, HashSet<string>>();
            foreach (TreeNode root in tvDb.Nodes)
            {
                foreach (TreeNode dbNode in root.Nodes)
                {
                    var checkedSet = new HashSet<string>();
                    foreach (TreeNode tableNode in dbNode.Nodes)
                    {
                        if (tableNode.Checked)
                        {
                            checkedSet.Add(tableNode.Text);
                        }
                    }

                    if (checkedSet.Count > 0)
                    {
                        result[dbNode.Text] = checkedSet;
                    }
                }
            }

            return result;
        }

        private void RestoreCheckedTables(Dictionary<string, HashSet<string>> saved)
        {
            foreach (TreeNode root in tvDb.Nodes)
            {
                foreach (TreeNode dbNode in root.Nodes)
                {
                    HashSet<string> checkedSet;
                    if (!saved.TryGetValue(dbNode.Text, out checkedSet) || checkedSet == null)
                    {
                        continue;
                    }

                    foreach (TreeNode tableNode in dbNode.Nodes)
                    {
                        if (checkedSet.Contains(tableNode.Text))
                        {
                            tableNode.Checked = true;
                        }
                    }
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // TablePanel 清理辅助
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 关闭当前连接下所有已打开的 TablePanel。
        /// </summary>
        private void CloseTablePanelsForConnection()
        {
            var keysToRemove = new ArrayList();
            foreach (DictionaryEntry item in TableMap)
            {
                var form = item.Value as TablePanel;
                if (form == null)
                {
                    continue;
                }

                form.Close();
                keysToRemove.Add(item.Key);
            }

            foreach (var key in keysToRemove)
            {
                TableMap.Remove(key);
            }
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
