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
using WeifenLuo.WinFormsUI.Docking;

namespace Banana.AutoCode.Forms
{
    public partial class TemplatePanel : DockContent
    {
        const string TEMPLATE_PATH = "Templates";
        const String DIRECTORY_ICON = "dir";

        public TemplatePanel()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            var iconList = new ImageList();
            var basePath = GetBasePath();
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
                Trace.WriteLine("Template directory:" + basePath + " not exists, auto created");
            }

            InitImageList(iconList, basePath);
            tvTemplates.ImageList = iconList;

            RefreshTreeView();
        }

        private string GetBasePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TEMPLATE_PATH);
        }

        protected void RefreshTreeView()
        {
            tvTemplates.Nodes.Clear();

            var basePath = GetBasePath();

            if (!Directory.Exists(basePath))
            {
                Trace.WriteLine("Template directory:" + basePath + " not exists");
                return;
            }

            var rootNode = new TreeNode();
            BuildFileTreeNode(rootNode, basePath);
            foreach (TreeNode node in rootNode.Nodes)
            {
                tvTemplates.Nodes.Add(node);
            }
        }

        private void BuildFileTreeNode(TreeNode currentNode, string path)
        {
            var dirs = Directory.GetDirectories(path);

            foreach (var dir in dirs)
            {
                var node = new TreeNode(GetFileName(dir));
                node.Tag = dir;
                node.ImageKey = DIRECTORY_ICON;
                node.SelectedImageKey = node.ImageKey;

                BuildFileTreeNode(node, dir);

                currentNode.Nodes.Add(node);
            }

            var files = Directory.GetFiles(path);
            
            foreach (var filePath in files)
            {
                var node = new TreeNode(GetFileName(filePath));
                node.Tag = filePath;
                node.ImageKey = GetExtension(filePath);
                node.SelectedImageKey = node.ImageKey;
                currentNode.Nodes.Add(node);
            }
        }

        private void InitImageList(ImageList iconList, string path)
        {
            if (! Directory.Exists(path))
            {
                return;
            }
            
            var baseIcon = IconUtils.GetIcon(path);
            if (baseIcon != null)
            {
                iconList.Images.Add(DIRECTORY_ICON, baseIcon);
            }
            
            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            foreach (var filePath in files)
            {
                var ext = GetExtension(filePath);
                if (! iconList.Images.ContainsKey(ext))
                {
                    iconList.Images.Add(ext, Icon.ExtractAssociatedIcon(filePath));
                }
            }
        }

        private string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        private string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }

        public override void Refresh()
        {
            base.Refresh();

            RefreshTreeView();
        }
    }
}
