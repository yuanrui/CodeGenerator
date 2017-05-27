using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Banana.AutoCode.DbSchema;
using WeifenLuo.WinFormsUI.Docking;

namespace Banana.AutoCode.Forms
{
    public partial class DbPanel : DockContent, IDisposable
    {
        protected ImageList IconList;
        protected ConnectionStringSettings CurrentConnSetting;
        protected Hashtable TableMap;

        public DbPanel()
        {
            InitializeComponent();
            Init();
        }

        protected void Init()
        {
            TableMap = new Hashtable();
            IconList = new ImageList();

            IconList.Images.Add("databases", Properties.Resources.datas);
            IconList.Images.Add("database", Properties.Resources.data);
            IconList.Images.Add("databaseOff", Properties.Resources.data_off);
            IconList.Images.Add("tables", Properties.Resources.tables);
            
            tvDb.ImageList = IconList;
            tvDb.DrawMode = TreeViewDrawMode.OwnerDrawText;
            tvDb.DrawNode += new DrawTreeNodeEventHandler(tvDb_DrawNode);
            tvDb.NodeMouseClick += new TreeNodeMouseClickEventHandler(tvDb_NodeMouseClick);

            Refresh();
        }

        protected void Refresh()
        {
            cbConnStrings.Items.Clear();
            tvDb.Nodes.Clear();
            
            foreach (DictionaryEntry item in TableMap)
            {
                var tableForm = item.Value as TablePanel;
                if (tableForm == null)
                {
                    continue;
                }

                tableForm.Close();
            }

            TableMap.Clear();
            cbConnStrings.Text = string.Empty;

            foreach (ConnectionStringSettings css in ConfigurationManager.ConnectionStrings)
            {
                cbConnStrings.Items.Add(css.Name);
            }
        }

        private void ClearOtherIcon(TreeNode currentNode, string imgOffKey)
        {
            if (currentNode == null)
            {
                return;
            }

            var pNode = currentNode.PrevNode;
            while (pNode != null)
            {
                pNode.ImageKey = imgOffKey;
                pNode.SelectedImageKey = imgOffKey;
                pNode = pNode.PrevNode;
            }

            var nNode = currentNode.NextNode;
            while (nNode != null)
            {
                nNode.ImageKey = imgOffKey;
                nNode.SelectedImageKey = imgOffKey;
                nNode = nNode.NextNode;
            }
        }

        void tvDb_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var table = e.Node.Tag as Table;

            var imgKey = "database";
            if (e.Node.Level == 1)
            {
                e.Node.SelectedImageKey = imgKey;
                ClearOtherIcon(e.Node, "databaseOff");
            }
            
            if (e.Node.Level == 2)
            {
                e.Node.Parent.ImageKey = imgKey;
                e.Node.Parent.SelectedImageKey = imgKey;
                var pNode = e.Node.Parent.PrevNode;

                ClearOtherIcon(e.Node.Parent, "databaseOff");

                if (table == null)
                {
                    return;
                }

                var tableForm = TableMap[table.ToString()] as TablePanel;
                if (tableForm == null)
                {
                    tableForm = new TablePanel(table, CurrentConnSetting);
                    TableMap[table.ToString()] = tableForm;
                    tableForm.Text = e.Node.Text;
                    tableForm.Show(((Main)this.ParentForm).MainDockPanel);
                }
                else
                {
                    tableForm.Activate();
                }
            }
        }
        
        void tvDb_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node.Level == 0 || e.Node.Level == 1)
            {
                e.Node.HideCheckBox();
            }

            e.DrawDefault = true;
        }

        private void cbConnStrings_SelectedIndexChanged(object sender, EventArgs e)
        {
            tvDb.Nodes.Clear();
            CurrentConnSetting = ConfigurationManager.ConnectionStrings[cbConnStrings.SelectedItem.ToString()];
            var dbSchemaManager = new DbSchemaManager(CurrentConnSetting);

            var dbs = dbSchemaManager.GetComplexDatabases();
            var root = new TreeNode(CurrentConnSetting.Name, 0, 0);
            root.ToolTipText = CurrentConnSetting.ConnectionString;
            root.ImageKey = "databases";
            root.SelectedImageKey = root.ImageKey;
            //root.TreeView.CheckBoxes = false;
            tvDb.Nodes.Add(root);
            tvDb.CheckBoxes = true;
            
            foreach (var db in dbs)
            {
                var dbNode = new TreeNode(db.DbName, 1, 1);
                dbNode.Tag = db;
                dbNode.ToolTipText = db.DbName;
                dbNode.ImageKey = "databaseOff";
                dbNode.SelectedImageKey = dbNode.ImageKey;

                foreach (var table in db.Tables)
                {
                    var tableNode = new TreeNode(table.Name, 2, 2);
                    tableNode.Tag = table;
                    tableNode.ToolTipText = string.IsNullOrWhiteSpace(table.Comment) ? table.Name : table.Comment;
                    tableNode.ImageKey = "tables";
                    tableNode.SelectedImageKey = tableNode.ImageKey;
                    dbNode.Nodes.Add(tableNode);
                }

                root.Nodes.Add(dbNode);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Refresh();
        }

        void IDisposable.Dispose()
        {
            this.TableMap.Clear();
            this.TableMap = null;

            this.Dispose(true);
        }
    }
}
