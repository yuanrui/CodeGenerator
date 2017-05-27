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
using Banana.AutoCode.DbSchema;

namespace Banana.AutoCode.Forms
{
    public partial class DbPanel : DockContent
    {
        protected ImageList IconList;

        public DbPanel()
        {
            InitializeComponent();
            Init();
        }

        protected void Init()
        {
            IconList = new ImageList();
            IconList.Images.Add("databases", Properties.Resources.datas);
            IconList.Images.Add("database", Properties.Resources.data);
            IconList.Images.Add("databaseOff", Properties.Resources.data_off);
            IconList.Images.Add("tables", Properties.Resources.tables);
           
            cbConnStrings.Items.Clear();
            tvDb.Nodes.Clear();
           
            tvDb.ImageList = IconList;

            foreach (ConnectionStringSettings css in ConfigurationManager.ConnectionStrings)
            {
                cbConnStrings.Items.Add(css.Name);
            }

            tvDb.DrawMode = TreeViewDrawMode.OwnerDrawText;
            tvDb.DrawNode += new DrawTreeNodeEventHandler(tvDb_DrawNode);
            
            tvDb.NodeMouseClick += new TreeNodeMouseClickEventHandler(tvDb_NodeMouseClick);
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
            var connSetting = ConfigurationManager.ConnectionStrings[cbConnStrings.SelectedItem.ToString()];
            var dbSchemaManager = new DbSchemaManager(connSetting);

            var dbs = dbSchemaManager.GetComplexDatabases();
            var root = new TreeNode(connSetting.Name, 0, 0);
            root.ToolTipText = connSetting.ConnectionString;
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
    }
}
